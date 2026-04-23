# QrZipRebuilder
QrZipRebuilder is a technical solution designed to bridge data transfer gaps in highly restricted environments. It enables the bidirectional conversion between compressed binary data (ZIP) and a sequence of visual data carriers (QR Codes).

# The Challenge
In many secure environments, direct file transfer (USB, Shared Folders, or Internet) is disabled, while visual output (monitors) and input (cameras/scanners) remain available. This project treats the screen and camera as a data bus.

# Key Features
Binary-to-Visual Encoding: Compresses and converts any ZIP file into a Base64 string, then fragments the data into a serialized sequence of QR Code images (PNG).

Visual-to-Binary Reconstruction: Captures and scans QR Code sequences from images or folders, reassembles the Base64 fragments in the correct order, and regenerates the original, bit-perfect ZIP file.

Data Integrity: Implements sequencing logic to ensure that even if QR codes are scanned out of order, the final file is reconstructed accurately.
