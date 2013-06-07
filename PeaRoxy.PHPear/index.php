<?php
#########################################################################################
#///////////////////////////////////////////////////////////////////////////////////////#
#////////			PHPear PeaRoxyWeb Server			////////#
#////////			� PeaRoxy Sep 2012				////////#
#////////			Version 1.0.3					////////#
#///////////////////////////////////////////////////////////////////////////////////////#
#########################################################################################
define("_Le_", TRUE);
require_once 'config.php';

$cryptor = false;
$peerCryptor = false;
$request = array();
$request['RequestBody'] = false;
$request['Host'] = false;
$request['EncryptionSalt'] = false;
$request['EncryptionKey'] = false;
$request['EncryptionType'] = 0;
if (count($_COOKIE) > 0)
{
    list($request['RequestInfo']) = array_values($_COOKIE);
    if (strlen($request['RequestInfo']) > 4)
    {
 		$request['RequestInfo'] = base64_decode($request['RequestInfo']);
		$request['EncryptionSalt'] = substr($request['RequestInfo'], 0, 4);
        $request['EncryptionType'] = ord(substr($request['RequestInfo'], 4, 1));
        $request['Host'] = substr($request['RequestInfo'], 5, strlen($request['RequestInfo']) - 5);
    }
    else
        DoError("Request info is corrupt.");
}
else
	DoError("Request info is missing.");

define("_CL_VALID_", TRUE);

switch ($request['EncryptionType'])
{
	case 0:
        if (Config::SupportedEncryptionTypes != 0 && Config::SupportedEncryptionTypes != -1)
            DoError("Unsupported encryption type.");
        break;
    case 2:
        if (Config::SupportedEncryptionTypes != 2 && Config::SupportedEncryptionTypes != -1)
            DoError("Unsupported encryption type.");
        break;
    default:
        DoError("Unsupported encryption type.");
        break;
}
$request['RequestBody'] = @file_get_contents("php://input");

if (!$request['RequestBody'])
    DoError("No request to handle.");

if (Config::AuthMethod == 1)
    if (isset($_SERVER['PHP_AUTH_USER']) && isset($_SERVER['PHP_AUTH_PW']))
    {
        $request['Username'] = strtolower($_SERVER['PHP_AUTH_USER']);
        $request['Password'] = $_SERVER['PHP_AUTH_PW'];
		$isFound = FALSE;
        foreach (Users::$Users as $u => $p) {
	        if (strtolower($u) == $request['Username'] && $request['Password'] == md5($p)) {
	            $request['EncryptionKey'] = $p;
				$isFound = TRUE;
				break;
	        }
	    }
		if (!$isFound)
			DoError("Authentication failed.");
	}
    else
    	DoError("This server need authentication.");

if (!$request['EncryptionKey'])
    $request['EncryptionKey'] = $request['EncryptionSalt'];
	
if ($request['EncryptionType'] == 2) // Simple XOR
{
	$peerCryptor = new SimpleXORCryptor($request['EncryptionKey']);
	$peerCryptor->SetIV($request['EncryptionSalt']);
	$request['Host'] = $peerCryptor->Decrypt($request['Host']);
	$request['RequestBody'] = $peerCryptor->Decrypt($request['RequestBody']);
}

if (Config::EncryptionType == 2) // Simple XOR
{
	if ($request['EncryptionType'] == 2) // Simple XOR
		$cryptor = $peerCryptor;
	else
	{
		$cryptor = new SimpleXORCryptor($request['EncryptionKey']);
		$cryptor->SetIV($request['EncryptionSalt']);
	}
}

$request['Encryption'] = FALSE;
$request['Compression'] = FALSE;
$request['Https'] = false;
if (stripos($request['Host'],"https")!==false)
{
    $request['Https'] = true;
    if (!extension_loaded('openssl') || !version_compare(PHP_VERSION, '4.3.0', '>='))
        DoError("No SSL support on this server.");
}

$protecolStartPoint = stripos($request['Host'],"://");
if ($protecolStartPoint!==false)
{
    $protecolStartPoint += 3;
    $request['Host'] = substr($request['Host'],$protecolStartPoint,strlen($request['Host']) - $protecolStartPoint);
}

$request['Port'] = 80;
$portStartPoint = strripos($request['Host'],":");
if ($portStartPoint!==false)
{
    $request['Port'] = substr($request['Host'],$portStartPoint + 1,strlen($request['Host']) - ($portStartPoint + 1));
    $request['Host'] = substr($request['Host'],0,$portStartPoint);
}

$errorNumber = false;
$errorMessage = false;
$connectionSocket = @fsockopen(($request['Https'] ? 'ssl://' : 'tcp://') . $request['Host'], $request['Port'], $errorNumber, $errorMessage, 30);
if ($connectionSocket === false)
    DoError("Connection failed, #" . $errorNumber . " " . $errorMessage);

@fwrite($connectionSocket, $request['RequestBody']);
@fflush($connectionSocket);

header("Content-Type: application/octet-stream");
$cookieKey = "";
list($cookieKey) = array_keys($_COOKIE);
header("Set-Cookie: " . $cookieKey . "=" . Config::EncryptionType . ";");
header("Transfer-encoding: chunked");

@ob_end_flush();
@ob_flush();
@flush();

$sockStatus = @socket_get_status($connectionSocket);
if (!$sockStatus)
	die;
set_time_limit(Config::NoDataConnectionTimeOut);
while (!$sockStatus['timed_out'] && !$sockStatus['eof'] && !@feof($connectionSocket) && !connection_aborted()) {
	$data = @fread($connectionSocket, Config::SendPacketSize);
	if ($data)
	{
		if (Config::EncryptionType == 2)
			$data = $cryptor->Encrypt($data);
		set_time_limit(Config::NoDataConnectionTimeOut);
		echo $data;
		@ob_end_flush();
		@ob_flush();
		@flush();
	}
	else
		break;
	$sockStatus = socket_get_status($connectionSocket);
}