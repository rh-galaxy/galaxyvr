<?php

	require_once("db_connect.php");
	require_once("checkparams.php");

	$level = $_GET["Level"];
	$userid = $_GET["UserId"];
	$paramArray = array($level, $userid);
	if(checkstring($paramArray) && $level!="" && $userid!="") {
		$db = connect_to_db();

		$query = "SELECT * FROM levels_t WHERE level='".$level."'";
		$result = mysqli_query($db, $query);

		$num_rows = @mysqli_num_rows($result);
		if($num_rows==1) {
			$levelrow = mysqli_fetch_row($result);
			$sort = "DESC";
			if($levelrow[2] != 0) {
				$sort = "ASC";
			}
			$query = "SELECT * FROM achievements_t WHERE level='".$level."' AND user_id='".$userid."' LIMIT 0,1";

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
