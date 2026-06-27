package com.example.swiftdropandroid.ui

import androidx.compose.foundation.layout.*
import androidx.compose.material3.Button
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp

@Composable
fun HomeScreen(
    modifier: Modifier = Modifier,
    selectedFile: String?,
    discoveredDevice: String?,
    onSelectFile: () -> Unit,
    onDiscover: () -> Unit,
    onConnect: () -> Unit
) {

    Column(
        modifier = modifier
            .fillMaxSize()
            .padding(24.dp),

        verticalArrangement = Arrangement.Center,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {

        Text(
            text = "SwiftDrop",
            fontSize = 32.sp,
            style = MaterialTheme.typography.headlineMedium
        )

        Spacer(modifier = Modifier.height(16.dp))

        Text("Super-fast local file sharing")

        Spacer(modifier = Modifier.height(40.dp))

        Button(
            onClick = onSelectFile,
            modifier = Modifier.fillMaxWidth()
        ) {
            Text("Select File")
        }

        Spacer(modifier = Modifier.height(16.dp))

        Button(
            onClick = onDiscover,
            modifier = Modifier.fillMaxWidth()
        ) {
            Text("Discover PC")
        }

        Spacer(modifier = Modifier.height(16.dp))

        Button(
            onClick = onConnect,
            modifier = Modifier.fillMaxWidth()
        ) {
            Text("Send File")
        }

        Spacer(modifier = Modifier.height(30.dp))

        if (selectedFile != null) {

            Text(
                text = "Selected File",
                style = MaterialTheme.typography.titleMedium
            )

            Spacer(modifier = Modifier.height(8.dp))

            Text(selectedFile)
        }

        Spacer(modifier = Modifier.height(24.dp))

        if (discoveredDevice != null) {

            Text(
                text = "Nearby Device",
                style = MaterialTheme.typography.titleMedium
            )

            Spacer(modifier = Modifier.height(8.dp))

            Text(discoveredDevice)
        }
    }
}