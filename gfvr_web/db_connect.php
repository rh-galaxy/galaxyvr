<?php

	function connect_to_db()
	{
		$db = mysqli_connect("mysql679.loopia.se", "php_user@g247094", pass_not_shown, "galaxy_forces_vr_com");

		return $db;
	}

?>
