<?php

	function connect_to_db_pc()
	{
		$db = mysqli_connect("mysql681.loopia.se", "php_user@g335301", pass_not_shown, "galaxy_forces_vr_com_db_3");

		return $db;
	}

?>
