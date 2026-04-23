================================================================================
  QR TO ZIP REBUILDER
  User guide (English)
================================================================================

OVERVIEW
--------
This application rebuilds a single ZIP archive from a set of photographs, each
showing a QR code. The tool is intended for situations where a file was split
across many QR images: each code carries a text fragment in Base64; the
software reads all fragments in the correct order, joins them, decodes the
data, and writes the recovered file to disk.

WHAT THE PROGRAM DOES
----------------------
  • You choose a folder that contains the image files (e.g. PNG, JPG).
  • The list of files is shown in a table, sorted in a stable order based on
    numbers appearing in the file names (e.g. image_1 before image_10).
  • When you start the recovery, each image is processed: the QR is detected
    and its text is read. Progress and status are written to a log.
  • If every image is read successfully, the combined Base64 is turned back
    into binary data and saved as "codigo_recuperado.zip" in the same folder
    you selected.
  • If any image fails, the archive is not created, so you always get a
    complete result or a clear error.

TYPICAL USE
-----------
Photograph a sequence of QR codes (for example from a screen or printout),
 place the photos in one folder, open this tool, point to that folder, and
run the rebuild. Keep the phone or camera steady, avoid glare, and make sure
each QR is fully visible and in focus.

REQUIREMENTS
------------
  • Microsoft Windows
  • .NET 8 runtime (or run from source with the .NET 8 SDK installed)

================================================================================
