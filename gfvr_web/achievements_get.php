<?php

	require_once("db_connect.php");
	require_once("checkparams.php");

	$user = $_GET["User"];
	$userid = $_GET["UserId"];
	$paramArray = array($user,$userid);
	$isok = 0; //no qualify

	
	if(checkstring($paramArray) && $user!="" && $userid!="") {
		$db = connect_to_db();

		$query = "SELECT * FROM members_t WHERE oculus_id=".$userid;
		$result_member = mysqli_query($db, $query);

		$num_rows = @mysqli_num_rows($result_member);
		if($num_rows==1) {
			$row = @mysqli_fetch_assoc($result_member);
			if($row['username'] == $user) {
				//user and id match
				$query = "UPDATE members_t SET last_access=NOW() WHERE (oculus_id=".$userid.")";
			} else {
				//update user name in members_t since it has changed
				$query = "UPDATE members_t SET username='".$user."',last_access=NOW() WHERE (oculus_id=".$userid.")";
			}
		} else if($num_rows==0) {
			//insert new row in members_t
			$query = "INSERT INTO members_t (username, oculus_id) VALUES('".$user."','".$userid."')";
		}
		$result = mysqli_query($db, $query);
		
		if($result) {
			//user id and name now exist in mebers_t, with last_access=NOW()
			
			$select_string = "SELECT levels_t.level AS Level, levels_t.is_time AS IsTime, levels_t.limit1,levels_t.limit2,levels_t.limit3 FROM levels_t ORDER BY levels_t.ordering ASC";
			// make query
			$result = @mysqli_query($db, $select_string);
			// succeeded
			if($result) {
				// count rows
				$num_rows = @mysqli_num_rows($result);
				// it better be more than 0
				if($num_rows > 0) {
					for($i=0; $i < $num_rows; $i++) {
						$score = -1;
						$row = @mysqli_fetch_assoc($result);
						$select_string = "SELECT achievements_t.score AS Score FROM achievements_t WHERE achievements_t.oculus_id=".$userid." AND achievements_t.level='".$row['Level']."'";

						$result2 = @mysqli_query($db, $select_string);
						// succeeded
						if($result2) {
							$num_rows2 = @mysqli_num_rows($result2);
							if($num_rows2 == 1) {
								// values
								$row2 = @mysqli_fetch_assoc($result2);
								$score = $row2['Score'];
							}
						}
						echo "\"".$row['Level']."\" ".$row['IsTime']." ".$row['limit1']." ".$row['limit2']." ".$row['limit3']." ".$score."\n";
					}
				}
			}
		}

		mysqli_close($db);
	}

?>
