
<?php

	require_once("db_connect.php");
	require_once("checkparams.php");

	$paramArray = array($_POST["LEVEL"], $_POST["NAME"], $_POST["USERID"], $_POST["COUNTER"], $_POST["SCORE"], $_POST["STEAM"], $_POST["REPLAY"]);

	if(checkstring($paramArray) && $_POST["LEVEL"]!="" && $_POST["NAME"]!="" && $_POST["USERID"]!="" && $_POST["COUNTER"]!="" && $_POST["SCORE"]!="" && $_POST["REPLAY"]!="") {
		$user = $_POST["NAME"];
		$userid = $_POST["USERID"];
		$steam = 0;
		if($_POST["STEAM"]==1) $steam = 1;
		$isok = 0; //no qualify

		$db = connect_to_db();

		$query = "SELECT * FROM members_t WHERE oculus_id='".$userid."'";
		$result_member = mysqli_query($db, $query);

		$num_rows = @mysqli_num_rows($result_member);
		if($num_rows==1) {
			$row = @mysqli_fetch_assoc($result_member);
			$count = $_POST["COUNTER"] ^ 1467;
			$tnow = time();
			$tpast = $row['last_access'];
			if(($tnow-$tpast)<$count+8 && ($tnow-$tpast)>$count-8 && $count>28) {
				$isok = 1;
			}
		}

		if($isok) {
			$query = "SELECT * FROM levels_t WHERE level='".$_POST["LEVEL"]."'";
			$level = mysqli_query($db, $query);

			$num_rows = @mysqli_num_rows($level);
			if($num_rows==1) {
				$levelrow = mysqli_fetch_row($level);
				$scoretobeat = -1000*1000;
				$sort = "DESC";
				if($levelrow[2] != 0) {
					$scoretobeat = 36000000;
					$sort = "ASC";
				}

				$query = "SELECT achievements_t.score FROM achievements_t WHERE level='".$_POST["LEVEL"]."' AND name='".$_POST["NAME"]."'";
				$hiscore = mysqli_query($db, $query);
				$num_rows = @mysqli_num_rows($hiscore);
				if($num_rows!=0) { //always qualify not 1
					$row1 = mysqli_fetch_row($hiscore);
					$scoretobeat = $row1[0]; //the score to beat
				}

				$qualify = 0;
				if($levelrow[2] != 0) {
					if($_POST["SCORE"] < $scoretobeat) $qualify = 1;
				} else {
					if($_POST["SCORE"] > $scoretobeat) $qualify = 1;
				}
				//limit to 4MB
				if(strlen($_POST["REPLAY"])>=4*1024*1024) $qualify = 0;

				if($qualify) {
					if(getenv("HTTP_CLIENT_IP") && strcasecmp(getenv("HTTP_CLIENT_IP"), "unknown")) {
						$ip = getenv("HTTP_CLIENT_IP");
					} else if(getenv("HTTP_X_FORWARDED_FOR") && strcasecmp(getenv("HTTP_X_FORWARDED_FOR"), "unknown")) {
						$ip = getenv("HTTP_X_FORWARDED_FOR");
					} else if(getenv("REMOTE_ADDR") && strcasecmp(getenv("REMOTE_ADDR"), "unknown")) {
						$ip = getenv("REMOTE_ADDR");
					} else if(isset($_SERVER['REMOTE_ADDR']) && $_SERVER['REMOTE_ADDR'] && strcasecmp($_SERVER['REMOTE_ADDR'], "unknown")) {
						$ip = $_SERVER['REMOTE_ADDR'];
					} else {
						$ip = "";
					}

					$replay = str_replace(' ','+',$_POST["REPLAY"]);
					if($num_rows!=0) {
						$query = "UPDATE achievements_t SET ip='".$ip."',timestamp=NOW(),score='".$_POST["SCORE"]."',replay='".$replay."' WHERE (level='".$_POST["LEVEL"]."' AND name='".$_POST["NAME"]."')";
					} else {
						$query = "INSERT INTO achievements_t (ip, user_id, name, level, score, replay, steam) VALUES('".$ip."','".$_POST["USERID"]."','".$_POST["NAME"]."','".$_POST["LEVEL"]."',".$_POST["SCORE"].",'".$replay."',".$steam.")";
					}
					$result = mysqli_query($db, $query);
				}

			}
		}
		
		if($db) mysqli_close($db);
	}

?>

<?php
	//update statistics
	require_once("db_connect.php");
	$db = connect_to_db();
	$query = "UPDATE statistics_t SET count=count+1 WHERE page='achievements_post.php'";
	$info = mysqli_query($db, $query);
	if($db) mysqli_close($db);
?>
