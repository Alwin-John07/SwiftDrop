package com.example.swiftdropandroid.protocol

import java.io.DataOutputStream

object ProtocolWriter {

    fun writeHeader(
        out: DataOutputStream,
        fileCount: Int
    ) {

        // Magic Header (4 bytes)
        out.writeBytes(ProtocolConstants.MAGIC_HEADER)

        // Protocol Version
        out.writeInt(
            ProtocolConstants.PROTOCOL_VERSION
        )

        // Number of files
        out.writeInt(fileCount)
    }

    fun writeFileInfo(
        out: DataOutputStream,
        fileName: String,
        fileSize: Long
    ) {

        val nameBytes =
            fileName.toByteArray(Charsets.UTF_8)

        out.writeInt(nameBytes.size)

        out.write(nameBytes)

        out.writeLong(fileSize)
    }

}