<?php

	require_once("db_connect.php");
	require_once("checkparams.php");

	$user = $_GET["User"];
	$paramArray = array($user);

	if(checkstring($paramArray) && $user!="") {
		
		$db = connect_to_db();

		$query = "SELECT * FROM mpgames_t WHERE username='".$user."'";
		$result = mysqli_query($db, $query);

		if(getenv("HTTP_CLIENT_IP") && strcasecmp(getenv("HTTP_CLIENT_IP"), "unknown")) {
			$ip = getenv("HTTP_CLIENT_IP");
		/*} else if(getenv("HTTP_X_FORWARDED_FOR") && strcasecmp(getenv("HTTP_X_FORWARDED_FOR"), "unknown")) {
			$ip = getenv("HTTP_X_FORWARDED_FOR");*/
		} else if(getenv("REMOTE_ADDR") && strcasecmp(getenv("REMOTE_ADDR"), "unknown")) {
			$ip = getenv("REMOTE_ADDR");
		} else if(isset($_SERVER['REMOTE_ADDR']) && $_SERVER['REMOTE_ADDR'] && strcasecmp($_SERVER['REMOTE_ADDR'], "unknown")) {
			$ip = $_SERVER['REMOTE_ADDR'];
		} else {
			$ip = "";
		}
		
		$num_rows = @mysqli_num_rows($result);
		if($num_rows==1) {
			$row = @mysqli_fetch_assoc($result);
			$query = "UPDATE mpgames_t SET ip='".$ip."', last_access=".time()." WHERE (username='".$user."')";
		} else if($num_rows==0) {
			//insert new row in mpgames_t
			$query = "INSERT INTO mpgames_t (username, ip, last_access) VALUES('".$user."','".$ip."',".time().")";
		}
		$result = mysqli_query($db, $query);
		
		if($result) {
			//username and ip now exist in mpgames_t, with last_access=NOW()
			
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
