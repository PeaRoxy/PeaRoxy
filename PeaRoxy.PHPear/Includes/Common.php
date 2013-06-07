<?php
#########################################################################################
#///////////////////////////////////////////////////////////////////////////////////////#
#////////			PHPear PeaRoxyWeb Server			////////#
#////////			� PeaRoxy Sep 2012				////////#
#////////			Version 1.0.3					////////#
#///////////////////////////////////////////////////////////////////////////////////////#
#########################################################################################
require_once dirname(dirname(__FILE__)).'/config.php';
function DoError($message){
	if (Config::RedirectURL && (!defined("_CL_VALID_") || !_CL_VALID_) && (!isset($_SERVER['HTTP_X_REQUESTED_WITH']) || $_SERVER['HTTP_X_REQUESTED_WITH'] != "NOREDIRECT"))
    {
        header("Location: " . Config::RedirectURL);
    }
	die("Server Error: " . $message);
}