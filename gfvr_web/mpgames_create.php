<?php

	require_once("db_connect.php");
	require_once("checkparams.php");

	$user = $_GET["User"];
	$ip = $_GET["IP"];
	$port = $_GET["Port"];
	$paramArray = array($user,$ip,$port);

	if(checkstring($paramArray) && $user!="" && $ip!="" && $port!="") {
		
		$db = connect_to_db();

		$query = "SELECT * FROM mpgames_t WHERE username='".$user."'";
		$result = mysqli_query($db, $query);

		$num_rows = @mysqli_num_rows($result);
		if($num_rows==1) {
			$row = @mysqli_fetch_assoc($result);
			$query = "UPDATE mpgames_t SET ip='".$ip."', port=".$port.", last_access=".time()." WHERE (username='".$user."')";
		} else if($num_rows==0) {
			//insert new row in mpgames_t
			$query = "INSERT INTO mpgames_t (username, ip, port, last_access) VALUES('".$user."','".$ip."',".$port.",".time().")";
		}
		$result = mysqli_query($db, $query);
		
		if($result) {
			//username, ip and port now exist in mpgames_t, with last_access=NOW()
			
			echo $ip;
		}
		mysqli_close($db);
	}

?>

<?php
	//update statistics
	require_once("db_connect.php");
	$db = connect_to_db();
	$query = "UPDATE statistics_t SET count=count+1 WHERE page='mpgames_create.php'";
	$info = mysqli_query($db, $query);
	if($db) mysqli_close($db);
?>
