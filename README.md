# MinecraftServerManager
This app helps to launch your server, create rotating local backups and restore them.
 
 ## Start your server
 Select your server .jar file, specify launch arguments in the config file and press Start to launch the server.
 ## Backup your server
 Create a .marker file in the folder where the backups should be stored, select it and press Backup.
 Backups are rotating, meaning that you can store several different backups, overriding the oldest when the maximum amount is reached. Default amount is 3 and is configurable.
 ## Restore backups
 With selected server jar and .marker file, press Restore to view the backups for your server. Double click on the backup in the list starts the restoration process, wiping the server folder and replacing it with the backup data.
