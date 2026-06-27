package com.example.swiftdropandroid.protocol

import java.io.DataInputStream
import java.io.IOException

data class IncomingFile(
    val fileName: String,
    val fileSize: Long
)

object ProtocolReader {

    fun readMagicHeader(input: DataInputStream) {

        val header = ByteArray(4)

        input.readFully(header)

        val magic = String(header, Charsets.UTF_8)

        if (magic != ProtocolConstants.MAGIC_HEADER) {
            throw IOException("Invalid SwiftDrop header.")
        }
    }

    fun readProtocolVersion(input: DataInputStream) {

        val version = input.readInt()

        if (version != ProtocolConstants.PROTOCOL_VERSION) {
            throw IOException("Unsupported protocol version: $version")
        }
    }

    fun readFileCount(input: DataInputStream): Int {
        return input.readInt()
    }

    fun readFileInfo(input: DataInputStream): IncomingFile {

        val fileNameLength = input.readInt()

        val fileNameBytes = ByteArray(fileNameLength)

        input.readFully(fileNameBytes)

        val fileName =
            String(fileNameBytes, Charsets.UTF_8)

        val fileSize =
            input.readLong()

        return IncomingFile(
            fileName = fileName,
            fileSize = fileSize
        )
    }

}