<?php

		// Philo : 
		// Si l'index existe, on récupère la valeur, on l'incrémente, on la sauve,
		// sinon on la cree à 1
		$hostname = "localhost";
		$username = "root";
		$password = "";
		$database = "mgmgeo"; // Si utilisation d'un point dans le login, le remplacer par "_" http://mon.login.free.fr -> mon_login
	 
		// L'index
		$today = date("Y-m-d");       
		
		$link = mysqli_connect("$hostname", "$username", "$password", "$database");
		// En cas d'erreur, on ne stoppe rien, on laisse télécharger le fichier
		if (mysqli_connect_errno()) 
		{
			printf("Connect failed: %s\n", mysqli_connect_error());
			exit();
		}
		
		// La gestion des cookies
		if(isset($_GET["cookie"]))
		{
			$cookie = $_GET["cookie"];
			// Est-ce que le cookie existe
			// ******************************
			$reqsql = "SELECT Id,Date FROM `COOKIES` WHERE `Id` LIKE '".$cookie."' LIMIT 0, 30 ";
			$result = mysqli_query($link, $reqsql);
			if (!$result) 
			{
				// error
			}
			else
			{
				$num_rows = mysqli_num_rows($result);
				if ($num_rows <> 0)
				{
					// Le cookie existe
					// On insère la nouvelle valeur
					$reqsql = "UPDATE  `mgmgeo`.`COOKIES` SET  `Date` =  '".$today."' WHERE  `COOKIES`.`Id` =  '".$cookie."' LIMIT 1 ;";
					$result = mysqli_query($link, $reqsql);
					if (!$result) 
					{
						//echo mysql_error();
					}
					else
					{
						// On a réussi
						//echo $hit;
					}
				}
				else
				{
					// on insère une nouvelle entrée à 1
					$reqsql = "INSERT INTO `mgmgeo`.`COOKIES` (`Id`, `Date`) VALUES ('".$cookie."', '".$today."');";
					$result = mysqli_query($link, $reqsql);
					if (!$result) 
					{
						//echo mysql_error();
					}
					else
					{
						// On a réussi
					}
				}
			}
			echo "OK";
		}
		else
		{
			// Hits normaux
			// Est-ce que la version existe ?
			// ******************************
			$reqsql = "SELECT Date,Hits FROM `HITS_PER_DAY` WHERE `Date` LIKE '".$today."' LIMIT 0, 30 ";
			$result = mysqli_query($link, $reqsql);
			if (!$result) 
			{
				// error
			}
			else
			{
				// La version existe
				$num_rows = mysqli_num_rows($result);
				$hit = 1;
				if ($num_rows <> 0)
				{
					// On récupère le résultat et on l'incrémente
					$row = mysqli_fetch_assoc($result); 
					mysqli_free_result($result);
					$hit = $row['Hits'] + 1;
					// On insère la nouvelle valeur
					$reqsql = "UPDATE  `mgmgeo`.`HITS_PER_DAY` SET  `Hits` =  '".$hit."' WHERE  `HITS_PER_DAY`.`Date` =  '".$today."' LIMIT 1 ;";
					$result = mysqli_query($link, $reqsql);
					if (!$result) 
					{
						//echo mysql_error();
					}
					else
					{
						// On a réussi
						//echo $hit;
					}
				}
				else
				{
					// on insère une nouvelle entrée à 1
					$reqsql = "INSERT INTO `mgmgeo`.`HITS_PER_DAY` (`Date`, `Hits`) VALUES ('".$today."', '1');";
					$result = mysqli_query($link, $reqsql);
					if (!$result) 
					{
						//echo mysql_error();
					}
					else
					{
						// On a réussi
						//echo "1";
					}
				}
			}
			//echo "NO:";
			echo ($today.':'.$hit);
		}
		
		mysqli_close($link);
?>
