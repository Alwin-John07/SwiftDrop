package com.example.swiftdropandroid.network

import android.os.Build
import com.example.swiftdropandroid.model.SwiftDropDevice
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.net.DatagramPacket
import java.net.DatagramSocket
import java.net.Inet4Address
import java.net.InetSocketAddress
import java.net.NetworkInterface
import java.net.SocketTimeoutException

object DiscoveryManager {

    private const val DISCOVERY_PORT = 5001

    suspend fun discover(timeout: Int = 5000): SwiftDropDevice? {

        return withContext(Dispatchers.IO) {

            try {

                DatagramSocket(null).use { socket ->

                    socket.broadcast = true
                    socket.reuseAddress = true
                    socket.bind(
                        InetSocketAddress(DISCOVERY_PORT)
                    )

                    val ownDeviceName =
                        Build.MODEL.replace("|", "")

                    val ownIpAddresses =
                        getLocalIpAddresses()

                    val buffer = ByteArray(1024)
                    val startedAt =
                        System.currentTimeMillis()

                    println("Listening for SwiftDrop devices...")

                    while (
                        System.currentTimeMillis() - startedAt < timeout
                    ) {

                        val remaining =
                            timeout - (
                                System.currentTimeMillis() - startedAt
                            ).toInt()

                        socket.soTimeout =
                            remaining
                                .coerceAtMost(500)
                                .coerceAtLeast(1)

                        val packet =
                            DatagramPacket(buffer, buffer.size)

                        try {

                            socket.receive(packet)

                        } catch (_: SocketTimeoutException) {

                            continue

                        }

                        val message = String(
                            packet.data,
                            0,
                            packet.length
                        )

                        println("Received: $message")

                        val parts = message.split("|")

                        if (parts.size != 3 || parts[0] != "SWIFTDROP")
                            continue

                        val ipAddress =
                            packet.address.hostAddress ?: continue

                        val name = parts[1]

                        val port =
                            parts[2].toIntOrNull() ?: continue

                        if (
                            isOwnAnnouncement(
                                name = name,
                                ipAddress = ipAddress,
                                ownDeviceName = ownDeviceName,
                                ownIpAddresses = ownIpAddresses
                            )
                        ) {

                            println(
                                "Ignoring own SwiftDrop broadcast: $message"
                            )

                            continue

                        }

                        return@withContext SwiftDropDevice(
                            name = name,
                            ipAddress = ipAddress,
                            port = port
                        )

                    }

                }

            } catch (e: Exception) {

                e.printStackTrace()

            }

            null

        }

    }

    private fun isOwnAnnouncement(
        name: String,
        ipAddress: String,
        ownDeviceName: String,
        ownIpAddresses: Set<String>
    ): Boolean {

        return name == ownDeviceName ||
            ipAddress in ownIpAddresses

    }

    private fun getLocalIpAddresses(): Set<String> {

        val addresses =
            mutableSetOf<String>()

        val interfaces =
            NetworkInterface.getNetworkInterfaces()
                ?: return addresses

        while (interfaces.hasMoreElements()) {

            val networkInterface =
                interfaces.nextElement()

            if (!networkInterface.isUp ||
                networkInterface.isLoopback)
            {
                continue
            }

            val inetAddresses =
                networkInterface.inetAddresses

            while (inetAddresses.hasMoreElements()) {

                val address =
                    inetAddresses.nextElement()

                if (address is Inet4Address) {
                    address.hostAddress?.let {
                        addresses.add(it)
                    }
                }

            }

        }

        return addresses

    }

}
