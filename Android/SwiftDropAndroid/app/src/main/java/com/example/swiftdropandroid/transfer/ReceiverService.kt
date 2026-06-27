package com.example.swiftdropandroid.transfer

import android.content.Context
import android.util.Log
import com.example.swiftdropandroid.protocol.ProtocolConstants
import com.example.swiftdropandroid.protocol.ProtocolReader
import com.example.swiftdropandroid.utilities.FileUtilities
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.isActive
import kotlinx.coroutines.withContext
import java.io.DataInputStream
import java.net.ServerSocket

object ReceiverService {

    suspend fun start(context: Context) =
        withContext(Dispatchers.IO) {

            Log.d("SwiftDropReceiver", "ReceiverService starting")

            val serverSocket =
                ServerSocket(ProtocolConstants.TRANSFER_PORT)

            Log.d(
                "SwiftDropReceiver",
                "Listening on port ${ProtocolConstants.TRANSFER_PORT}"
            )

            try {

                while (isActive) {

                    Log.d(
                        "SwiftDropReceiver",
                        "Waiting for sender..."
                    )

                    val socket = serverSocket.accept()

                    Log.d(
                        "SwiftDropReceiver",
                        "TCP connection accepted"
                    )

                    try {

                        val input =
                            DataInputStream(socket.getInputStream())

                        Log.d(
                            "SwiftDropReceiver",
                            "Reading magic header..."
                        )

                        ProtocolReader.readMagicHeader(input)

                        Log.d(
                            "SwiftDropReceiver",
                            "Magic header OK"
                        )

                        Log.d(
                            "SwiftDropReceiver",
                            "Reading protocol version..."
                        )

                        ProtocolReader.readProtocolVersion(input)

                        Log.d(
                            "SwiftDropReceiver",
                            "Protocol version OK"
                        )

                        val fileCount =
                            ProtocolReader.readFileCount(input)

                        Log.d(
                            "SwiftDropReceiver",
                            "File count = $fileCount"
                        )

                        repeat(fileCount) {

                            val file =
                                ProtocolReader.readFileInfo(input)

                            Log.d(
                                "SwiftDropReceiver",
                                "Receiving ${file.fileName}"
                            )

                            val uri =
                                FileUtilities.createDownloadFile(
                                    context,
                                    file.fileName
                                )

                            context.contentResolver
                                .openOutputStream(uri)
                                ?.use { output ->

                                    val buffer =
                                        ByteArray(
                                            ProtocolConstants.BUFFER_SIZE
                                        )

                                    var remaining =
                                        file.fileSize

                                    while (remaining > 0) {

                                        val bytesToRead =
                                            minOf(
                                                buffer.size.toLong(),
                                                remaining
                                            ).toInt()

                                        val bytesRead =
                                            input.read(
                                                buffer,
                                                0,
                                                bytesToRead
                                            )

                                        if (bytesRead == -1)
                                            throw Exception(
                                                "Connection lost."
                                            )

                                        output.write(
                                            buffer,
                                            0,
                                            bytesRead
                                        )

                                        remaining -= bytesRead
                                    }

                                    output.flush()
                                }

                            FileUtilities.finalizeFile(
                                context,
                                uri
                            )

                            Log.d(
                                "SwiftDropReceiver",
                                "${file.fileName} received successfully"
                            )
                        }

                        input.close()

                    } catch (e: Exception) {

                        Log.e(
                            "SwiftDropReceiver",
                            "Receiver error",
                            e
                        )

                    } finally {

                        socket.close()

                    }
                }

            } finally {

                serverSocket.close()

            }

        }
}