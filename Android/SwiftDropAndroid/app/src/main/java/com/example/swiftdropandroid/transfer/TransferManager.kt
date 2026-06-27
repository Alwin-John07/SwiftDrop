package com.example.swiftdropandroid.transfer

import android.content.Context
import android.net.Uri
import android.provider.OpenableColumns
import android.util.Log
import com.example.swiftdropandroid.network.ConnectionManager
import com.example.swiftdropandroid.protocol.ProtocolConstants
import com.example.swiftdropandroid.protocol.ProtocolWriter
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.io.DataOutputStream

object TransferManager {

    private const val TAG = "SwiftDropSender"

    suspend fun sendFile(
        context: Context,
        uri: Uri,
        fileName: String
    ): Boolean {

        return withContext(Dispatchers.IO) {

            try {

                Log.d(TAG, "Opening TCP connection...")

                val socket =
                    ConnectionManager.openConnection()

                Log.d(
                    TAG,
                    "Connected to ${socket.inetAddress.hostAddress}:${socket.port}"
                )

                val fileSize =
                    getFileSize(context, uri)
                        ?: return@withContext false

                Log.d(TAG, "File name = $fileName")
                Log.d(TAG, "File size = $fileSize")

                DataOutputStream(
                    socket.getOutputStream()
                ).use { out ->

                    context.contentResolver
                        .openInputStream(uri)
                        ?.use { input ->

                            ProtocolWriter.writeHeader(
                                out,
                                1
                            )

                            Log.d(TAG, "Header sent")

                            ProtocolWriter.writeFileInfo(
                                out,
                                fileName,
                                fileSize
                            )

                            Log.d(TAG, "File info sent")

                            val buffer =
                                ByteArray(
                                    ProtocolConstants.BUFFER_SIZE
                                )

                            var totalSent = 0L

                            while (true) {

                                val bytesRead =
                                    input.read(buffer)

                                if (bytesRead == -1)
                                    break

                                out.write(
                                    buffer,
                                    0,
                                    bytesRead
                                )

                                totalSent += bytesRead
                            }

                            Log.d(
                                TAG,
                                "Total bytes sent = $totalSent"
                            )

                            if (totalSent != fileSize) {
                                throw IllegalStateException(
                                    "Expected $fileSize bytes, sent $totalSent bytes."
                                )
                            }

                            out.flush()

                            Log.d(TAG, "Flush complete")

                        } ?: return@withContext false

                }

                socket.close()

                Log.d(TAG, "Socket closed")

                true

            }
            catch (e: Exception)
            {
                Log.e(
                    TAG,
                    "Transfer failed",
                    e
                )

                false
            }
        }
    }

    private fun getFileSize(
        context: Context,
        uri: Uri
    ): Long? {

        context.contentResolver
            .query(uri, null, null, null, null)
            ?.use { cursor ->

                val index =
                    cursor.getColumnIndex(
                        OpenableColumns.SIZE
                    )

                if (index >= 0 && cursor.moveToFirst()) {

                    val size =
                        cursor.getLong(index)

                    if (size >= 0)
                        return size

                }

            }

        context.contentResolver
            .openFileDescriptor(uri, "r")
            ?.use { descriptor ->

                if (descriptor.statSize >= 0)
                    return descriptor.statSize

            }

        context.contentResolver
            .openInputStream(uri)
            ?.use { input ->

                val buffer =
                    ByteArray(
                        ProtocolConstants.BUFFER_SIZE
                    )

                var total = 0L

                while (true) {

                    val bytesRead =
                        input.read(buffer)

                    if (bytesRead == -1)
                        break

                    total += bytesRead

                }

                return total

            }

        return null

    }
}
