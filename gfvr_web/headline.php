<?php
require_once("square.php");

function headline($name)
{
	square_start("'0'");
	echo "<h2>".$name."</h2>";
	square_end();
}

?>
