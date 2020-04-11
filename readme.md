Android backup unpacker
=======================

Tool written in C# for extracting data from android backup.

# Features
* Convert backup to TAR archive
* Unpack backup content to folder
* Support for encrypted backups

# Usage
Convert backup to TAR archive  
```abu backup.ab --convert archive.tar```    

Extract backup content to folder  
```abu backup.ab --unpack folder```    

Convert encrypted backup to TAR archive  
```abu encrypted.ab --convert archive.tar --password 1234```    

Extract encrypted backup content to folder  
```abu encrypted.ab --unpack folder --password 1234```  

### Thank you
Nikolay Elenkov  
https://github.com/nelenkov/android-backup-extractor  
https://nelenkov.blogspot.com/2012/06/unpacking-android-backups.html  