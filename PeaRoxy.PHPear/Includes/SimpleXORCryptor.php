<?php
#########################################################################################
#///////////////////////////////////////////////////////////////////////////////////////#
#////////			PHPear PeaRoxyWeb Server			////////#
#////////			� PeaRoxy Sep 2012				////////#
#////////			Version 1.0.3					////////#
#///////////////////////////////////////////////////////////////////////////////////////#
#########################################################################################
require_once dirname(dirname(__FILE__)).'/config.php';
class SimpleXORCryptor{
	private $_iv = "";
	private $_key = "";
	private $keyPos_enc = 0;
	private $keyPos_dec = 0;
	
	public function __construct($key)
	{
		$this->_key = $key;
	}
	
	public function SetIV($iv){
		for ($i=0; $i < strlen($this->_key); $i++){
			$nv = ord($this->_key[$i]) ^ ord($iv[($i + 1) % strlen($iv)]);
			if ($nv != 0)
				$this->_key[$i] = chr($nv);
		}
		$this->_iv = $iv;
	}
		
	public function Encrypt($toEncrypt){
		$resultString = "";
		for ($i=0; $i < strlen($toEncrypt); $i++)
			$resultString .= chr(ord($toEncrypt[$i]) ^ ord($this->_key[($i + $this->keyPos_enc) % strlen($this->_key)]));
		$this->keyPos_enc = ($this->keyPos_enc + strlen($toEncrypt)) % strlen($this->_key);
		return $resultString;
	}
		
	public function Decrypt($toEncrypt){
		$resultString = "";
		for ($i=0; $i < strlen($toEncrypt); $i++)
			$resultString .= chr(ord($toEncrypt[$i]) ^ ord($this->_key[($i + $this->keyPos_dec) % strlen($this->_key)]));
		$this->keyPos_dec = ($this->keyPos_dec + strlen($toEncrypt)) % strlen($this->_key);
		return $resultString;
	}
}