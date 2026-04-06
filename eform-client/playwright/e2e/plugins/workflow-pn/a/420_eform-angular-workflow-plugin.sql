-- MySQL dump 10.13  Distrib 8.0.23, for Win64 (x86_64)
--
-- Host: localhost    Database: 420_eform-angular-workflow-plugin
-- ------------------------------------------------------
-- Server version	8.0.23

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `__EFMigrationsHistory`
--

DROP TABLE IF EXISTS `__EFMigrationsHistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8 */;
CREATE TABLE `__EFMigrationsHistory` (
  `MigrationId` varchar(150) NOT NULL,
  `ProductVersion` varchar(32)  NOT NULL,
  PRIMARY KEY (`MigrationId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `__EFMigrationsHistory`
--

LOCK TABLES `__EFMigrationsHistory` WRITE;
/*!40000 ALTER TABLE `__EFMigrationsHistory` DISABLE KEYS */;
INSERT INTO `__EFMigrationsHistory` VALUES ('20210621182502_InitialCreate','5.0.7');
/*!40000 ALTER TABLE `__EFMigrationsHistory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `PluginConfigurationValues`
--

DROP TABLE IF EXISTS `PluginConfigurationValues`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8 */;
CREATE TABLE `PluginConfigurationValues` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` longtext,
  `Value` longtext,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `WorkflowState` varchar(255) DEFAULT NULL,
  `CreatedByUserId` int NOT NULL,
  `UpdatedByUserId` int NOT NULL,
  `Version` int NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `PluginConfigurationValues`
--

LOCK TABLES `PluginConfigurationValues` WRITE;
/*!40000 ALTER TABLE `PluginConfigurationValues` DISABLE KEYS */;
INSERT INTO `PluginConfigurationValues` VALUES (1,'WorkflowBaseSettings:FirstEformId','0','2021-06-21 18:40:58.446267','2021-06-21 18:40:58.446456','created',1,0,1),(2,'WorkflowBaseSettings:SecondEformId','0','2021-06-21 18:40:58.598450','2021-06-21 18:40:58.598451','created',1,0,1);
/*!40000 ALTER TABLE `PluginConfigurationValues` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `PluginConfigurationValueVersions`
--

DROP TABLE IF EXISTS `PluginConfigurationValueVersions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8 */;
CREATE TABLE `PluginConfigurationValueVersions` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` longtext,
  `Value` longtext,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `WorkflowState` varchar(255) DEFAULT NULL,
  `CreatedByUserId` int NOT NULL,
  `UpdatedByUserId` int NOT NULL,
  `Version` int NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `PluginConfigurationValueVersions`
--

LOCK TABLES `PluginConfigurationValueVersions` WRITE;
/*!40000 ALTER TABLE `PluginConfigurationValueVersions` DISABLE KEYS */;
/*!40000 ALTER TABLE `PluginConfigurationValueVersions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `PluginGroupPermissions`
--

DROP TABLE IF EXISTS `PluginGroupPermissions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8 */;
CREATE TABLE `PluginGroupPermissions` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `GroupId` int NOT NULL,
  `PermissionId` int NOT NULL,
  `IsEnabled` tinyint(1) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `WorkflowState` varchar(255) DEFAULT NULL,
  `CreatedByUserId` int NOT NULL,
  `UpdatedByUserId` int NOT NULL,
  `Version` int NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `IX_PluginGroupPermissions_PermissionId` (`PermissionId`) USING BTREE,
  CONSTRAINT `FK_PluginGroupPermissions_PluginPermissions_PermissionId` FOREIGN KEY (`PermissionId`) REFERENCES `PluginPermissions` (`Id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `PluginGroupPermissions`
--

LOCK TABLES `PluginGroupPermissions` WRITE;
/*!40000 ALTER TABLE `PluginGroupPermissions` DISABLE KEYS */;
INSERT INTO `PluginGroupPermissions` VALUES (1,1,1,1,'2021-06-21 18:40:58.762467','2021-06-21 18:40:58.762468','created',0,0,1);
/*!40000 ALTER TABLE `PluginGroupPermissions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `PluginGroupPermissionVersions`
--

DROP TABLE IF EXISTS `PluginGroupPermissionVersions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8 */;
CREATE TABLE `PluginGroupPermissionVersions` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `GroupId` int NOT NULL,
  `PermissionId` int NOT NULL,
  `IsEnabled` tinyint(1) NOT NULL,
  `PluginGroupPermissionId` int NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `WorkflowState` varchar(255) DEFAULT NULL,
  `CreatedByUserId` int NOT NULL,
  `UpdatedByUserId` int NOT NULL,
  `Version` int NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `PluginGroupPermissionVersions`
--

LOCK TABLES `PluginGroupPermissionVersions` WRITE;
/*!40000 ALTER TABLE `PluginGroupPermissionVersions` DISABLE KEYS */;
INSERT INTO `PluginGroupPermissionVersions` VALUES (1,1,1,1,1,'2021-06-21 18:40:58.762467','2021-06-21 18:40:58.762468','created',0,0,1);
/*!40000 ALTER TABLE `PluginGroupPermissionVersions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `PluginPermissions`
--

DROP TABLE IF EXISTS `PluginPermissions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8 */;
CREATE TABLE `PluginPermissions` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `PermissionName` longtext,
  `ClaimName` longtext,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `WorkflowState` varchar(255) DEFAULT NULL,
  `CreatedByUserId` int NOT NULL,
  `UpdatedByUserId` int NOT NULL,
  `Version` int NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `PluginPermissions`
--

LOCK TABLES `PluginPermissions` WRITE;
/*!40000 ALTER TABLE `PluginPermissions` DISABLE KEYS */;
INSERT INTO `PluginPermissions` VALUES (1,'Access Workflow Plugin','workflow_plugin_access','2021-06-21 18:40:58.617381',NULL,'created',1,0,1);
/*!40000 ALTER TABLE `PluginPermissions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `WorkflowCases`
--

DROP TABLE IF EXISTS `WorkflowCases`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8 */;
CREATE TABLE `WorkflowCases` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Description` longtext,
  `DateOfIncident` datetime(6) NOT NULL,
  `IncidentType` longtext,
  `IncidentPlace` longtext,
  `Deadline` datetime(6) NOT NULL,
  `ActionPlan` longtext,
  `SolvedBy` longtext,
  `Status` longtext,
  `PhotosExist` tinyint(1) NOT NULL,
  `MicrotingId` int NOT NULL,
  `CheckMicrotingUid` int NOT NULL,
  `CheckId` int NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `WorkflowState` varchar(255) DEFAULT NULL,
  `CreatedByUserId` int NOT NULL,
  `UpdatedByUserId` int NOT NULL,
  `Version` int NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `WorkflowCases`
--

LOCK TABLES `WorkflowCases` WRITE;
/*!40000 ALTER TABLE `WorkflowCases` DISABLE KEYS */;
INSERT INTO `WorkflowCases` VALUES (1,'47fb0240-8dc5-43b5-bd17-d31a4eb844d7','2021-06-21 21:04:51.000000','731dd508-d3d2-41cd-b9b1-0753a3de57e9','d14edf67-e864-4661-a3ba-6ce2d2c5c95d','2021-06-27 21:05:08.000000','b1335fac-8f7d-4f76-a525-3c737e0ada5f','','No status',0,0,0,0,'2021-06-21 21:05:46.000000','2021-06-21 21:05:48.000000','created',1,1,1),(2,'b8bbea8a-5e1b-4d5e-a890-61ef42d49da1','2021-06-15 21:04:51.000000','06235274-40ca-43be-9adc-eeb99865d7a2','6fe95f58-845c-4784-ae97-d1a738b155ce','2021-06-30 21:05:08.000000','d732d2bd-d6b5-470e-a27f-422d2e364029','','Closed',1,0,0,0,'2021-06-21 21:05:46.000000','2021-06-21 21:05:50.000000','created',1,1,1),(3,'b4068568-4bc5-405a-a92f-7d4a3080fc6b','2021-06-26 21:04:51.000000','9971c397-61a9-4b6d-aa68-38bf30123360','d23fa495-e8d0-4545-840d-62446f16fe99','2021-07-01 21:05:08.000000','591f85e9-bd96-4ed2-9041-ce6d335e79fb','','Ongoing',0,0,0,0,'2021-06-21 21:05:46.000000','2021-06-21 21:05:30.000000','created',1,1,1);
/*!40000 ALTER TABLE `WorkflowCases` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `WorkflowCaseVersions`
--

DROP TABLE IF EXISTS `WorkflowCaseVersions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8 */;
CREATE TABLE `WorkflowCaseVersions` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `WorkflowCaseId` int NOT NULL,
  `Description` longtext,
  `DateOfIncident` datetime(6) NOT NULL,
  `IncidentType` longtext,
  `IncidentPlace` longtext,
  `Deadline` datetime(6) NOT NULL,
  `ActionPlan` longtext,
  `SolvedBy` longtext,
  `Status` longtext,
  `PhotosExist` tinyint(1) NOT NULL,
  `MicrotingId` int NOT NULL,
  `CheckMicrotingUid` int NOT NULL,
  `CheckId` int NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `WorkflowState` varchar(255) DEFAULT NULL,
  `CreatedByUserId` int NOT NULL,
  `UpdatedByUserId` int NOT NULL,
  `Version` int NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `WorkflowCaseVersions`
--

LOCK TABLES `WorkflowCaseVersions` WRITE;
/*!40000 ALTER TABLE `WorkflowCaseVersions` DISABLE KEYS */;
/*!40000 ALTER TABLE `WorkflowCaseVersions` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2021-06-22 17:44:21
