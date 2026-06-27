package com.example.swiftdropandroid
import android.util.Log
import com.example.swiftdropandroid.transfer.ReceiverService
import android.net.Uri
import android.os.Bundle
import android.provider.OpenableColumns
import android.widget.Toast
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Scaffold
import androidx.compose.runtime.mutableStateOf
import androidx.compose.ui.Modifier
import androidx.lifecycle.lifecycleScope
import com.example.swiftdropandroid.network.ConnectionManager
import com.example.swiftdropandroid.network.DiscoveryBroadcaster
import com.example.swiftdropandroid.network.DiscoveryManager
import com.example.swiftdropandroid.transfer.TransferManager
import com.example.swiftdropandroid.ui.HomeScreen
import com.example.swiftdropandroid.ui.theme.SwiftDropAndroidTheme
import kotlinx.coroutines.launch


class MainActivity : ComponentActivity() {

    private var selectedFileName = mutableStateOf<String?>(null)
    private var selectedFileUri: Uri? = null

    private var discoveredDevice = mutableStateOf<String?>(null)
    private var discoveredIp: String? = null

    private val filePicker =
        registerForActivityResult(ActivityResultContracts.GetContent()) { uri: Uri? ->

            if (uri != null) {
                selectedFileUri = uri
                selectedFileName.value = getFileName(uri)
            }

        }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        enableEdgeToEdge()

        // Start broadcasting this Android device
        lifecycleScope.launch {
            DiscoveryBroadcaster.start(this@MainActivity)
        }
        lifecycleScope.launch {

            try {

                ReceiverService.start(this@MainActivity)

            }
            catch (e: Exception)
            {
                Log.e(
                    "SwiftDropReceiver",
                    "Receiver failed to start",
                    e
                )
            }
        }

        setContent {

            SwiftDropAndroidTheme {

                Scaffold(
                    modifier = Modifier.fillMaxSize()
                ) { innerPadding ->

                    HomeScreen(
                        modifier = Modifier.padding(innerPadding),

                        selectedFile = selectedFileName.value,

                        discoveredDevice = discoveredDevice.value,

                        onSelectFile = {
                            filePicker.launch("*/*")
                        },

                        onDiscover = {

                            lifecycleScope.launch {

                                val device = DiscoveryManager.discover()

                                if (device != null) {

                                    discoveredDevice.value =
                                        "${device.name}\n${device.ipAddress}"

                                    discoveredIp = device.ipAddress

                                    Toast.makeText(
                                        this@MainActivity,
                                        "Device Found!",
                                        Toast.LENGTH_SHORT
                                    ).show()

                                } else {

                                    Toast.makeText(
                                        this@MainActivity,
                                        "No SwiftDrop receiver found.",
                                        Toast.LENGTH_SHORT
                                    ).show()

                                }

                            }

                        },

                        onConnect = {

                            lifecycleScope.launch {

                                if (selectedFileUri == null || selectedFileName.value == null) {

                                    Toast.makeText(
                                        this@MainActivity,
                                        "Please select a file first.",
                                        Toast.LENGTH_SHORT
                                    ).show()

                                    return@launch

                                }

                                ConnectionManager.setHost(
                                    discoveredIp ?: "10.0.2.2"
                                )

                                val success = TransferManager.sendFile(
                                    context = this@MainActivity,
                                    uri = selectedFileUri!!,
                                    fileName = selectedFileName.value!!
                                )

                                Toast.makeText(
                                    this@MainActivity,
                                    if (success) "File Sent!" else "Transfer Failed",
                                    Toast.LENGTH_SHORT
                                ).show()

                            }

                        }

                    )

                }

            }

        }

    }

    private fun getFileName(uri: Uri): String {

        var result = "Unknown File"

        contentResolver.query(uri, null, null, null, null)?.use { cursor ->

            val index = cursor.getColumnIndex(OpenableColumns.DISPLAY_NAME)

            if (cursor.moveToFirst()) {
                result = cursor.getString(index)
            }

        }

        return result
    }

}