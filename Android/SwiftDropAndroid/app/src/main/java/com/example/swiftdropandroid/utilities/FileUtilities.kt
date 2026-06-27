package com.example.swiftdropandroid.utilities

import android.content.ContentValues
import android.content.Context
import android.net.Uri
import android.os.Environment
import android.provider.MediaStore

object FileUtilities {

    fun createDownloadFile(
        context: Context,
        fileName: String
    ): Uri {

        val values = ContentValues().apply {

            put(
                MediaStore.Downloads.DISPLAY_NAME,
                fileName
            )

            put(
                MediaStore.Downloads.MIME_TYPE,
                "application/octet-stream"
            )

            put(
                MediaStore.Downloads.RELATIVE_PATH,
                Environment.DIRECTORY_DOWNLOADS + "/SwiftDrop"
            )

            put(
                MediaStore.Downloads.IS_PENDING,
                1
            )
        }

        return context.contentResolver.insert(
            MediaStore.Downloads.EXTERNAL_CONTENT_URI,
            values
        ) ?: throw Exception("Unable to create output file.")
    }

    fun finalizeFile(
        context: Context,
        uri: Uri
    ) {

        val values = ContentValues().apply {

            put(
                MediaStore.Downloads.IS_PENDING,
                0
            )

        }

        context.contentResolver.update(
            uri,
            values,
            null,
            null
        )
    }
}