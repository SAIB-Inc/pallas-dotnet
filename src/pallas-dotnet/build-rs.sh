#!/bin/bash

# Function to build and copy for Linux
build_for_linux() {
    cargo build --release --manifest-path ../pallas-dotnet-rs/n2c-miniprotocols/Cargo.toml
    cargo build --release --manifest-path ../pallas-dotnet-rs/n2n-miniprotocols/Cargo.toml
    cp ../pallas-dotnet-rs/n2c-miniprotocols/target/release/libpallas_dotnet_n2c.so "./libpallas_dotnet_n2c.so"
    cp ../pallas-dotnet-rs/n2n-miniprotocols/target/release/libpallas_dotnet_n2n.so "./libpallas_dotnet_n2n.so"
    rnet-gen ../pallas-dotnet-rs/n2c-miniprotocols/target/release/libpallas_dotnet_n2c.so > "./NodeToClientWrapper.cs"
    rnet-gen ../pallas-dotnet-rs/n2n-miniprotocols/target/release/libpallas_dotnet_n2n.so > "./NodeToNodeWrapper.cs"
}

# Function to build and copy for macOS
build_for_macos() {
    cargo build --release --manifest-path ../pallas-dotnet-rs/n2c-miniprotocols/Cargo.toml
    cargo build --release --manifest-path ../pallas-dotnet-rs/n2n-miniprotocols/Cargo.toml
    cp ../pallas-dotnet-rs/n2c-miniprotocols/target/release/libpallas_dotnet_n2c.dylib "./libpallas_dotnet_n2c.dylib"
    cp ../pallas-dotnet-rs/n2n-miniprotocols/target/release/libpallas_dotnet_n2n.dylib "./libpallas_dotnet_n2n.dylib"
    rnet-gen ../pallas-dotnet-rs/n2c-miniprotocols/target/release/libpallas_dotnet_n2c.dylib > "./NodeToClientWrapper.cs"
    rnet-gen ../pallas-dotnet-rs/n2n-miniprotocols/target/release/libpallas_dotnet_n2n.dylib > "./NodeToNodeWrapper.cs"
}

# Check the operating system
OS="`uname`"
case $OS in
  'Linux')
    # Linux-specific commands
    build_for_linux
    ;;
  'Darwin')
    # macOS-specific commands
    build_for_macos
    ;;
  *) ;;
esac
