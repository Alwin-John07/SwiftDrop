package com.example.swiftdropandroid.network

import java.net.Socket

object ConnectionManager {

    private var host = "10.0.2.2"

    private const val PORT = 5000

    fun setHost(ip: String) {
        host = ip
    }

    fun getHost(): String {
        return host
    }

    fun openConnection(): Socket {
        return Socket(host, PORT)
    }

}