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
		error_reporting(E_ERROR | E_WARNING | E_PARSE | E_NOTICE);
		
		$link = mysql_connect("$hostname", "$username", "$password");
		// En cas d'erreur, on ne stoppe rien, on laisse télécharger le fichier
		if (!$link) 
		{
			printf("Connect failed: %s\n", mysql_error());
			exit();
		}
		
		$dbcheck = mysql_select_db("$database");
		if (!$dbcheck) 
		{
			printf("Connect failed: %s\n", mysql_error());
			exit();
		}
		
		// La gestion des cookies
		if(isset($_GET["cookie"]))
		{
			$cookie = $_GET["cookie"];
			// Est-ce que le cookie existe
			// ******************************
			$reqsql = "SELECT Id,Date FROM `COOKIES` WHERE `Id` LIKE '".$cookie."' LIMIT 0, 30 ";
			$result = mysql_query($reqsql, $link);
			if (!$result) 
			{
				// error
			}
			else
			{
				$num_rows = mysql_num_rows($result);
				if ($num_rows <> 0)
				{
					// Le cookie existe
					// On insère la nouvelle valeur
					$reqsql = "UPDATE  `mgmgeo`.`COOKIES` SET  `Date` =  '".$today."' WHERE  `COOKIES`.`Id` =  '".$cookie."' LIMIT 1 ;";
					$result = mysql_query($reqsql, $link);
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
					$result = mysql_query($reqsql, $link);
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
			$result = mysql_query($reqsql, $link);
			if (!$result) 
			{
				// error
			}
			else
			{
				// La version existe
				$num_rows = mysql_num_rows($result);
				$hit = 1;
				if ($num_rows <> 0)
				{
					// On récupère le résultat et on l'incrémente
					$row = mysql_fetch_assoc($result); 
					mysql_free_result($result);
					$hit = $row['Hits'] + 1;
					// On insère la nouvelle valeur
					$reqsql = "UPDATE  `mgmgeo`.`HITS_PER_DAY` SET  `Hits` =  '".$hit."' WHERE  `HITS_PER_DAY`.`Date` =  '".$today."' LIMIT 1 ;";
					$result = mysql_query($reqsql, $link);
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
					$result = mysql_query($reqsql, $link);
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
		
		mysql_close($link);
?>
