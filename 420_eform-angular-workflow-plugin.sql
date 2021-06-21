/*
 Navicat Premium Data Transfer

 Source Server         : MySql
 Source Server Type    : MySQL
 Source Server Version : 80023
 Source Host           : localhost:3306
 Source Schema         : 420_eform-angular-workflow-plugin

 Target Server Type    : MySQL
 Target Server Version : 80023
 File Encoding         : 65001

 Date: 21/06/2021 22:47:21
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for __efmigrationshistory
-- ----------------------------
DROP TABLE IF EXISTS `__efmigrationshistory`;
CREATE TABLE `__efmigrationshistory`  (
  `MigrationId` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductVersion` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`MigrationId`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of __efmigrationshistory
-- ----------------------------
INSERT INTO `__efmigrationshistory` VALUES ('20210621182502_InitialCreate', '5.0.7');

-- ----------------------------
-- Table structure for pluginconfigurationvalues
-- ----------------------------
DROP TABLE IF EXISTS `pluginconfigurationvalues`;
CREATE TABLE `pluginconfigurationvalues`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  `Value` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) NULL DEFAULT NULL,
  `WorkflowState` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `CreatedByUserId` int NOT NULL,
  `UpdatedByUserId` int NOT NULL,
  `Version` int NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of pluginconfigurationvalues
-- ----------------------------
INSERT INTO `pluginconfigurationvalues` VALUES (1, 'WorkflowBaseSettings:FirstEformId', '0', '2021-06-21 18:40:58.446267', '2021-06-21 18:40:58.446456', 'created', 1, 0, 1);
INSERT INTO `pluginconfigurationvalues` VALUES (2, 'WorkflowBaseSettings:SecondEformId', '0', '2021-06-21 18:40:58.598450', '2021-06-21 18:40:58.598451', 'created', 1, 0, 1);

-- ----------------------------
-- Table structure for pluginconfigurationvalueversions
-- ----------------------------
DROP TABLE IF EXISTS `pluginconfigurationvalueversions`;
CREATE TABLE `pluginconfigurationvalueversions`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  `Value` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) NULL DEFAULT NULL,
  `WorkflowState` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `CreatedByUserId` int NOT NULL,
  `UpdatedByUserId` int NOT NULL,
  `Version` int NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of pluginconfigurationvalueversions
-- ----------------------------

-- ----------------------------
-- Table structure for plugingrouppermissions
-- ----------------------------
DROP TABLE IF EXISTS `plugingrouppermissions`;
CREATE TABLE `plugingrouppermissions`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `GroupId` int NOT NULL,
  `PermissionId` int NOT NULL,
  `IsEnabled` tinyint(1) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) NULL DEFAULT NULL,
  `WorkflowState` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `CreatedByUserId` int NOT NULL,
  `UpdatedByUserId` int NOT NULL,
  `Version` int NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `IX_PluginGroupPermissions_PermissionId`(`PermissionId`) USING BTREE,
  CONSTRAINT `FK_PluginGroupPermissions_PluginPermissions_PermissionId` FOREIGN KEY (`PermissionId`) REFERENCES `pluginpermissions` (`Id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 2 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of plugingrouppermissions
-- ----------------------------
INSERT INTO `plugingrouppermissions` VALUES (1, 1, 1, 1, '2021-06-21 18:40:58.762467', '2021-06-21 18:40:58.762468', 'created', 0, 0, 1);

-- ----------------------------
-- Table structure for plugingrouppermissionversions
-- ----------------------------
DROP TABLE IF EXISTS `plugingrouppermissionversions`;
CREATE TABLE `plugingrouppermissionversions`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `GroupId` int NOT NULL,
  `PermissionId` int NOT NULL,
  `IsEnabled` tinyint(1) NOT NULL,
  `PluginGroupPermissionId` int NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) NULL DEFAULT NULL,
  `WorkflowState` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `CreatedByUserId` int NOT NULL,
  `UpdatedByUserId` int NOT NULL,
  `Version` int NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 2 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of plugingrouppermissionversions
-- ----------------------------
INSERT INTO `plugingrouppermissionversions` VALUES (1, 1, 1, 1, 1, '2021-06-21 18:40:58.762467', '2021-06-21 18:40:58.762468', 'created', 0, 0, 1);

-- ----------------------------
-- Table structure for pluginpermissions
-- ----------------------------
DROP TABLE IF EXISTS `pluginpermissions`;
CREATE TABLE `pluginpermissions`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `PermissionName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  `ClaimName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) NULL DEFAULT NULL,
  `WorkflowState` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `CreatedByUserId` int NOT NULL,
  `UpdatedByUserId` int NOT NULL,
  `Version` int NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 2 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of pluginpermissions
-- ----------------------------
INSERT INTO `pluginpermissions` VALUES (1, 'Access Workflow Plugin', 'workflow_plugin_access', '2021-06-21 18:40:58.617381', NULL, 'created', 1, 0, 1);

-- ----------------------------
-- Table structure for workflowcases
-- ----------------------------
DROP TABLE IF EXISTS `workflowcases`;
CREATE TABLE `workflowcases`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Description` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  `DateOfIncident` datetime(6) NOT NULL,
  `IncidentType` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  `IncidentPlace` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  `Deadline` datetime(6) NOT NULL,
  `ActionPlan` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  `SolvedBy` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  `Status` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  `PhotosExist` tinyint(1) NOT NULL,
  `MicrotingId` int NOT NULL,
  `CheckMicrotingUid` int NOT NULL,
  `CheckId` int NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) NULL DEFAULT NULL,
  `WorkflowState` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `CreatedByUserId` int NOT NULL,
  `UpdatedByUserId` int NOT NULL,
  `Version` int NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 4 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of workflowcases
-- ----------------------------
INSERT INTO `workflowcases` VALUES (1, '47fb0240-8dc5-43b5-bd17-d31a4eb844d7', '2021-06-21 21:04:51.000000', '731dd508-d3d2-41cd-b9b1-0753a3de57e9', 'd14edf67-e864-4661-a3ba-6ce2d2c5c95d', '2021-06-27 21:05:08.000000', 'b1335fac-8f7d-4f76-a525-3c737e0ada5f', '', 'No status', 0, 0, 0, 0, '2021-06-21 21:05:46.000000', '2021-06-21 21:05:48.000000', 'created', 1, 1, 1);
INSERT INTO `workflowcases` VALUES (2, 'b8bbea8a-5e1b-4d5e-a890-61ef42d49da1', '2021-06-15 21:04:51.000000', '06235274-40ca-43be-9adc-eeb99865d7a2', '6fe95f58-845c-4784-ae97-d1a738b155ce', '2021-06-30 21:05:08.000000', 'd732d2bd-d6b5-470e-a27f-422d2e364029', '', 'Closed', 1, 0, 0, 0, '2021-06-21 21:05:46.000000', '2021-06-21 21:05:50.000000', 'created', 1, 1, 1);
INSERT INTO `workflowcases` VALUES (3, 'b4068568-4bc5-405a-a92f-7d4a3080fc6b', '2021-06-26 21:04:51.000000', '9971c397-61a9-4b6d-aa68-38bf30123360', 'd23fa495-e8d0-4545-840d-62446f16fe99', '2021-07-01 21:05:08.000000', '591f85e9-bd96-4ed2-9041-ce6d335e79fb', '', 'Ongoing', 0, 0, 0, 0, '2021-06-21 21:05:46.000000', '2021-06-21 21:05:30.000000', 'created', 1, 1, 1);

-- ----------------------------
-- Table structure for workflowcaseversions
-- ----------------------------
DROP TABLE IF EXISTS `workflowcaseversions`;
CREATE TABLE `workflowcaseversions`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `WorkflowCaseId` int NOT NULL,
  `Description` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  `DateOfIncident` datetime(6) NOT NULL,
  `IncidentType` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  `IncidentPlace` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  `Deadline` datetime(6) NOT NULL,
  `ActionPlan` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  `SolvedBy` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  `Status` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  `PhotosExist` tinyint(1) NOT NULL,
  `MicrotingId` int NOT NULL,
  `CheckMicrotingUid` int NOT NULL,
  `CheckId` int NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) NULL DEFAULT NULL,
  `WorkflowState` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `CreatedByUserId` int NOT NULL,
  `UpdatedByUserId` int NOT NULL,
  `Version` int NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of workflowcaseversions
-- ----------------------------

SET FOREIGN_KEY_CHECKS = 1;
