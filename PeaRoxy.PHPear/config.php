<?php
#########################################################################################
#///////////////////////////////////////////////////////////////////////////////////////#
#////////			PHPear PeaRoxyWeb Server			////////#
#////////			� PeaRoxy Sep 2012				////////#
#////////			Version 1.0.3					////////#
#///////////////////////////////////////////////////////////////////////////////////////#
#########################################################################################
final class Config{
	#	Setting for PeaRoxy web script
		
		# Active Method for clients to authenticate
		# Edit users.ini for users list
		# 0: No Auth / 1: User & Pass / Default: 0
	const AuthMethod=0;
		
		# Server side encryption type
		# Apply to sending only (Clients must set their own encryption settings)
		# 0: No Enc / 2: SimpleXor Stream Self-Sync Encryption / Default: 0
	const EncryptionType=0;
		
		# Acceptable encryptions
		# Limit encryption types that user can use, If you have high load it is recommended to limit users encryption
		# 0: Only No Enc / 2: Only SimpleXor / -1: Anything / Default: -1
	const SupportedEncryptionTypes=-1;
		
        # Max packet size of each information (Only Clients)
	    # Apply to each connection separately
	    # (Byte) Default: 1024 (1 KB) - 8192 (8 KB)
    const SendPacketSize=8192;
		
		# When to close a timeout connection
		# Close connection if no data transferred
		# (Second) Default: 300 (5 Min)
	const NoDataConnectionTimeOut=300;

        # URL for redirecting users when they didn't used PeaRoxy to connect to us.
        # (WebAddress) Default: 0 (Disable)
    const RedirectURL=0;
        
		# Create error log file
	    # Will create log file for errors
	    # 0: False / 1: True
    const LogErrors=1;

		# You are done. Don't edit anything else.
	private function __construct(){}
}
	
if (!defined("_Le_") || !_Le_)
	die("Access denied");
error_reporting(E_ALL);
set_time_limit(0);
ini_set("display_errors" , "1");
if(ini_get('zlib.output_compression')){ 
	ini_set('zlib.output_compression', 'Off'); 
}
if(ini_get('output_buffering')){ 
	ini_set('output_buffering', 'Off'); 
}
if (Config::LogErrors == 1)
{
    ini_set("log_errors" , "1");
    ini_set("error_log" , "Errors.log");
}
require_once 'Includes/Common.php';
require_once 'Includes/SimpleXORCryptor.php';
require_once 'users.php';