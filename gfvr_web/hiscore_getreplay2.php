<?php

	require_once("db_connect.php");
	require_once("checkparams.php");

	$Level = $_GET["Level"];
	$Name = $_GET["Name"];
	$paramArray = array($Level, $Name);

	if(checkstring($paramArray) && $Level!="" && $Name!="") {
		$db = connect_to_db();

		$query = "SELECT * FROM levels_t WHERE level='".$Level."'";
		$level = mysqli_query($db, $query);

		$num_rows = @mysqli_num_rows($level);
		if($num_rows==1) {
			$levelrow = mysqli_fetch_row($level);
			$sort = "DESC";
			if($levelrow[2] != 0) {
				$sort = "ASC";
			}
			$query = "SELECT * FROM achievements_t WHERE level='".$Level."' AND name='".$Name."' LIMIT 0,1";
			$hiscore = mysqli_query($db, $query);
			$num_rows = @mysqli_num_rows($hiscore);
			if($num_rows==1) {
				$row = mysqli_fetch_row($hiscore);
				print_r($row[5]); //the wanted hiscore blob
			}
		}

		if($db) mysqli_close($db);
	}

?>
