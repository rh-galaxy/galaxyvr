<?php

function checkstring($strArray) {
   //,"@","_" ,"(",")","-"
   $wrongchars=array("~","`","!","#","\$","%","^","&","*","+","=","|","\\","{","}",":",";","\"","'",",","<",".",">","?","/");

   foreach ($strArray as $postvars) {
      foreach ($wrongchars as $val) {
         $wrong=strchr($postvars,$val);
         if($wrong!=false) {
            return false;
         }
      }
   }
   return true; //assume valid
}
?>
