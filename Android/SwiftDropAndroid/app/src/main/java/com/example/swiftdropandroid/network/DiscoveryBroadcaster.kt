package com.example.swiftdropandroid.network

import android.content.Context
import android.os.Build
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.delay
import kotlinx.coroutines.isActive
import kotlinx.coroutines.withContext
import java.net.DatagramPacket
import java.net.DatagramSocket
import java.net.InetAddress

object DiscoveryBroadcaster {

    private const val DISCOVERY_PORT = 5001
    private const val RECEIVER_PORT = 5000

    suspend fun start(context: Context) =
        withContext(Dispatchers.IO) {

            val socket = DatagramSocket()
            socket.broadcast = true

            val deviceName =
                Build.MODEL.replace("|", "")

            val message =
                "SWIFTDROP|$deviceName|$RECEIVER_PORT"

            val data = message.toByteArray()

            val address =
                InetAddress.getByName("255.255.255.255")

            try
            {
                while (isActive)
                {
                    val packet =
                        DatagramPacket(
                            data,
                            data.size,
                            address,
                            DISCOVERY_PORT
                        )

                    while (true) {

                        try {

                            socket.send(packet)

                        } catch (e: Exception) {

                            println("Discovery broadcast skipped: ${e.message}")

                        }

                        delay(2000)
                    }

                    delay(1000)
                }
            }
            finally
            {
                socket.close()
            }
        }
}