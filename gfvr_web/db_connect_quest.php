<?php

	function connect_to_db_quest()
	{
		$db = mysqli_connect("mysql462.loopia.se", "php_user@g285632", "pass_not_shown", "galaxy_forces_vr_com_db_1");

		return $db;
	}

?>
