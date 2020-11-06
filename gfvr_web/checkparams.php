<?php

function checkstring($strArray) {
   //,"@","_" ,"(",")","-"
   $wrongchars=array("~","`","!","#","\$","%","^","&","*","+","=","|","\\","{","}",":",";","\"","'",",","<",".",">","?","/");

   while(list(,$postvars)=each($strArray)) {
      while(list(,$val)=each($wrongchars)) {
         $wrong=strchr($postvars,$val);
         if($wrong!=false) {
            return false;
         }
      }
   }
   return true; //assume valid
}
?>
