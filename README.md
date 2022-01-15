# SharpEncrypt
Application for encrypting and decrypting files.

This is entirely a personal project and coding exercise and is not intended for commercial use.
There is no guarantee the encryption methods used will protect your files from hackers, this project was merely done for fun to practice designing and implementing ideas.

## How to Use

When first launching the application after installation, it will ask you to provide a **master key** file.
This is simply a 16-byte file that is used for certain parts of the encryption process.
You can create a new text file and type 16 characters in it, and that will suffice as a basic master key.
The key thing to note is files encrypted with a master key must also be decrypted with that same master key, otherwise the process will not work and file corruption is possible.
If you share an encrypted file with someone else, make sure they use the same master key to decrypt it.

Now you can open files in the application to either encrypt or decrypt them.
Use the "open folder" or "open files" buttons to load files.
Then, simply click either the encrypt or decrypt button on the right side.
You can follow the progress of the application with the progress bar at the bottom and the output window on the left.

Several additional options can also be configured on the right side of the window, such as encrypting filenames.
Other options exist on the menu at the top of the window.
Under "File", you can change your master key file or shut down the application.
Under "Options", you can tell the application to log its output to a file and enable/disable context menu items for Windows Explorer.
If logging is enabled, log files will be created in the directory "[your user]/AppData/Local/mpagliaro98/SharpEncrypt/logs".
If context menu items are enabled, when you right-click a file or folder in Windows Explorer, you will see options to encrypt and decrypt.
Clicking these will automatically open the application and load those files in.
