/*M!999999\- enable the sandbox mode */ 
-- MariaDB dump 10.19-11.7.2-MariaDB, for Win64 (AMD64)
--
-- Host: localhost    Database: policedb
-- ------------------------------------------------------
-- Server version	8.0.45

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*M!100616 SET @OLD_NOTE_VERBOSITY=@@NOTE_VERBOSITY, NOTE_VERBOSITY=0 */;

--
-- Table structure for table `arrests`
--

DROP TABLE IF EXISTS `arrests`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `arrests` (
  `id` int NOT NULL AUTO_INCREMENT,
  `arrest_number` varchar(30) NOT NULL,
  `suspect_id` int NOT NULL,
  `case_id` int NOT NULL,
  `arresting_officer_id` int NOT NULL,
  `arrest_date` datetime NOT NULL,
  `charges` text NOT NULL,
  `status` enum('detained','released','charged','convicted') DEFAULT 'detained',
  PRIMARY KEY (`id`),
  UNIQUE KEY `arrest_number` (`arrest_number`),
  KEY `fk_arrests_suspect` (`suspect_id`),
  KEY `fk_arrests_case` (`case_id`),
  KEY `fk_arrests_officer` (`arresting_officer_id`),
  CONSTRAINT `fk_arrests_case` FOREIGN KEY (`case_id`) REFERENCES `cases` (`Case_ID`),
  CONSTRAINT `fk_arrests_officer` FOREIGN KEY (`arresting_officer_id`) REFERENCES `user` (`User_ID`),
  CONSTRAINT `fk_arrests_suspect` FOREIGN KEY (`suspect_id`) REFERENCES `suspect_list` (`Suspect_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `arrests`
--

LOCK TABLES `arrests` WRITE;
/*!40000 ALTER TABLE `arrests` DISABLE KEYS */;
/*!40000 ALTER TABLE `arrests` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `audit_log`
--

DROP TABLE IF EXISTS `audit_log`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `audit_log` (
  `Log_ID` int NOT NULL,
  `User_ID` int NOT NULL,
  PRIMARY KEY (`Log_ID`),
  KEY `User_ID` (`User_ID`),
  CONSTRAINT `audit_log_ibfk_1` FOREIGN KEY (`User_ID`) REFERENCES `user` (`User_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `audit_log`
--

LOCK TABLES `audit_log` WRITE;
/*!40000 ALTER TABLE `audit_log` DISABLE KEYS */;
/*!40000 ALTER TABLE `audit_log` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `case_assignment`
--

DROP TABLE IF EXISTS `case_assignment`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `case_assignment` (
  `Assignment_ID` int NOT NULL,
  `AssignmentDate` date NOT NULL,
  `AssignmentStatus` varchar(20) NOT NULL,
  `Case_ID` int NOT NULL,
  PRIMARY KEY (`Assignment_ID`),
  KEY `Case_ID` (`Case_ID`),
  CONSTRAINT `case_assignment_ibfk_1` FOREIGN KEY (`Case_ID`) REFERENCES `cases` (`Case_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `case_assignment`
--

LOCK TABLES `case_assignment` WRITE;
/*!40000 ALTER TABLE `case_assignment` DISABLE KEYS */;
/*!40000 ALTER TABLE `case_assignment` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `cases`
--

DROP TABLE IF EXISTS `cases`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `cases` (
  `Case_ID` int NOT NULL,
  `Case_Type` enum('theft','assault','fraud','homicide','traffic','other') NOT NULL,
  `Title` varchar(100) NOT NULL,
  `Description` varchar(512) NOT NULL,
  `Status` enum('open','under_investigation','closed','archived') NOT NULL,
  `OpenDate` date NOT NULL,
  `CloseDate` date NOT NULL,
  `Priority` enum('low','medium','high','critical') DEFAULT 'low',
  PRIMARY KEY (`Case_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `cases`
--

LOCK TABLES `cases` WRITE;
/*!40000 ALTER TABLE `cases` DISABLE KEYS */;
/*!40000 ALTER TABLE `cases` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `complaint`
--

DROP TABLE IF EXISTS `complaint`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `complaint` (
  `Complaint_ID` int NOT NULL,
  `Category` varchar(20) NOT NULL,
  `Description` varchar(255) NOT NULL,
  `Date` date NOT NULL,
  `Officer_ID` int NOT NULL,
  PRIMARY KEY (`Complaint_ID`),
  KEY `Officer_ID` (`Officer_ID`),
  CONSTRAINT `complaint_ibfk_1` FOREIGN KEY (`Officer_ID`) REFERENCES `user` (`User_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `complaint`
--

LOCK TABLES `complaint` WRITE;
/*!40000 ALTER TABLE `complaint` DISABLE KEYS */;
/*!40000 ALTER TABLE `complaint` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `departments`
--

DROP TABLE IF EXISTS `departments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `departments` (
  `Department_ID` int NOT NULL,
  `Department_Name` varchar(255) NOT NULL,
  `Description` varchar(512) NOT NULL,
  PRIMARY KEY (`Department_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `departments`
--

LOCK TABLES `departments` WRITE;
/*!40000 ALTER TABLE `departments` DISABLE KEYS */;
INSERT INTO `departments` VALUES
(1,'IT','Testing Department');
/*!40000 ALTER TABLE `departments` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `dna_analysis`
--

DROP TABLE IF EXISTS `dna_analysis`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `dna_analysis` (
  `Analysis_ID` int NOT NULL,
  `Status` varchar(20) NOT NULL,
  `Methodology` varchar(20) NOT NULL,
  `Sample_Condition` varchar(20) NOT NULL,
  `Findings` varchar(512) NOT NULL,
  `Conclusion` varchar(255) NOT NULL,
  `Date_Received` date NOT NULL,
  `Date_Analyzed` date NOT NULL,
  `Condition_on_Arrival` varchar(20) NOT NULL,
  `Forensic_Expert_ID` int NOT NULL,
  `Investigation_ID` int NOT NULL,
  PRIMARY KEY (`Investigation_ID`,`Analysis_ID`),
  KEY `Forensic_Expert_ID` (`Forensic_Expert_ID`),
  CONSTRAINT `dna_analysis_ibfk_1` FOREIGN KEY (`Forensic_Expert_ID`) REFERENCES `user` (`User_ID`),
  CONSTRAINT `dna_analysis_ibfk_2` FOREIGN KEY (`Investigation_ID`) REFERENCES `investigation_report` (`Investigation_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `dna_analysis`
--

LOCK TABLES `dna_analysis` WRITE;
/*!40000 ALTER TABLE `dna_analysis` DISABLE KEYS */;
/*!40000 ALTER TABLE `dna_analysis` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `driving_license`
--

DROP TABLE IF EXISTS `driving_license`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `driving_license` (
  `License_ID` int NOT NULL,
  `User_ID` int NOT NULL,
  PRIMARY KEY (`License_ID`),
  KEY `User_ID` (`User_ID`),
  CONSTRAINT `driving_license_ibfk_1` FOREIGN KEY (`User_ID`) REFERENCES `user` (`User_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `driving_license`
--

LOCK TABLES `driving_license` WRITE;
/*!40000 ALTER TABLE `driving_license` DISABLE KEYS */;
/*!40000 ALTER TABLE `driving_license` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `evidence`
--

DROP TABLE IF EXISTS `evidence`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `evidence` (
  `id` int NOT NULL AUTO_INCREMENT,
  `evidence_number` varchar(30) NOT NULL,
  `case_id` int NOT NULL,
  `evidence_type` enum('physical','digital','document','biological','other') DEFAULT NULL,
  `description` text NOT NULL,
  `collection_date` datetime NOT NULL,
  `status` enum('collected','in_analysis','analyzed','stored','released') DEFAULT 'collected',
  `collected_by` int DEFAULT NULL,
  `analyzed_by` int DEFAULT NULL,
  `chain_of_custody` text,
  PRIMARY KEY (`id`),
  UNIQUE KEY `evidence_number` (`evidence_number`),
  KEY `fk_evidence_case` (`case_id`),
  KEY `fk_evidence_collected_by` (`collected_by`),
  KEY `fk_evidence_analyzed_by` (`analyzed_by`),
  CONSTRAINT `fk_evidence_analyzed_by` FOREIGN KEY (`analyzed_by`) REFERENCES `user` (`User_ID`),
  CONSTRAINT `fk_evidence_case` FOREIGN KEY (`case_id`) REFERENCES `cases` (`Case_ID`),
  CONSTRAINT `fk_evidence_collected_by` FOREIGN KEY (`collected_by`) REFERENCES `user` (`User_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `evidence`
--

LOCK TABLES `evidence` WRITE;
/*!40000 ALTER TABLE `evidence` DISABLE KEYS */;
/*!40000 ALTER TABLE `evidence` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `fingerprint_analysis`
--

DROP TABLE IF EXISTS `fingerprint_analysis`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `fingerprint_analysis` (
  `Analysis_ID` int NOT NULL,
  `Status` varchar(20) NOT NULL,
  `Print_Quality` varchar(20) NOT NULL,
  `Comparison_Results` varchar(255) NOT NULL,
  `Identification_Status` varchar(20) NOT NULL,
  `Conclusions` varchar(512) NOT NULL,
  `Date_Received` date NOT NULL,
  `Date_Analyzed` date NOT NULL,
  `Condition_on_arrival` varchar(20) NOT NULL,
  `Forensic_Expert_ID` int NOT NULL,
  `Investigation_ID` int NOT NULL,
  PRIMARY KEY (`Investigation_ID`,`Analysis_ID`),
  KEY `Forensic_Expert_ID` (`Forensic_Expert_ID`),
  CONSTRAINT `fingerprint_analysis_ibfk_1` FOREIGN KEY (`Forensic_Expert_ID`) REFERENCES `user` (`User_ID`),
  CONSTRAINT `fingerprint_analysis_ibfk_2` FOREIGN KEY (`Investigation_ID`) REFERENCES `investigation_report` (`Investigation_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `fingerprint_analysis`
--

LOCK TABLES `fingerprint_analysis` WRITE;
/*!40000 ALTER TABLE `fingerprint_analysis` DISABLE KEYS */;
/*!40000 ALTER TABLE `fingerprint_analysis` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `incident_report`
--

DROP TABLE IF EXISTS `incident_report`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `incident_report` (
  `Report_ID` int NOT NULL,
  `Type` varchar(20) NOT NULL,
  `Location` varchar(255) NOT NULL,
  `Description` varchar(512) NOT NULL,
  `Date` date NOT NULL,
  `Time` varchar(20) NOT NULL,
  `Arrest_Record` varchar(20) DEFAULT NULL,
  `Officer_ID` int NOT NULL,
  `Case_ID` int DEFAULT NULL,
  `Status` enum('open','linked','resolved') DEFAULT 'open',
  PRIMARY KEY (`Report_ID`),
  KEY `Officer_ID` (`Officer_ID`),
  KEY `Case_ID` (`Case_ID`),
  CONSTRAINT `incident_report_ibfk_1` FOREIGN KEY (`Officer_ID`) REFERENCES `user` (`User_ID`),
  CONSTRAINT `incident_report_ibfk_2` FOREIGN KEY (`Case_ID`) REFERENCES `cases` (`Case_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `incident_report`
--

LOCK TABLES `incident_report` WRITE;
/*!40000 ALTER TABLE `incident_report` DISABLE KEYS */;
/*!40000 ALTER TABLE `incident_report` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `investigation_report`
--

DROP TABLE IF EXISTS `investigation_report`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `investigation_report` (
  `Investigation_ID` int NOT NULL,
  `Investigation_Status` varchar(20) NOT NULL,
  `Summary` varchar(255) NOT NULL,
  `Evidence_analysis` varchar(255) NOT NULL,
  `Suspect_assessment` varchar(255) NOT NULL,
  `Investigative_Conclusions` varchar(255) NOT NULL,
  `Evidence` varchar(255) NOT NULL,
  `Report_ID` int NOT NULL,
  `Detective_ID` int NOT NULL,
  PRIMARY KEY (`Investigation_ID`),
  KEY `Report_ID` (`Report_ID`),
  KEY `Detective_ID` (`Detective_ID`),
  CONSTRAINT `investigation_report_ibfk_1` FOREIGN KEY (`Report_ID`) REFERENCES `incident_report` (`Report_ID`),
  CONSTRAINT `investigation_report_ibfk_2` FOREIGN KEY (`Detective_ID`) REFERENCES `user` (`User_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `investigation_report`
--

LOCK TABLES `investigation_report` WRITE;
/*!40000 ALTER TABLE `investigation_report` DISABLE KEYS */;
/*!40000 ALTER TABLE `investigation_report` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `login_otp`
--

DROP TABLE IF EXISTS `login_otp`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `login_otp` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `User_ID` int DEFAULT NULL,
  `Code` varchar(10) DEFAULT NULL,
  `Expiry` datetime DEFAULT NULL,
  `IsUsed` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `User_ID` (`User_ID`),
  CONSTRAINT `login_otp_ibfk_1` FOREIGN KEY (`User_ID`) REFERENCES `user` (`User_ID`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `login_otp`
--

LOCK TABLES `login_otp` WRITE;
/*!40000 ALTER TABLE `login_otp` DISABLE KEYS */;
INSERT INTO `login_otp` VALUES
(1,1,'393437','2026-05-07 02:15:59',0),
(2,1,'205972','2026-05-07 02:18:49',0),
(3,1,'134321','2026-05-07 02:32:17',0),
(4,1,'407819','2026-05-07 09:07:48',0),
(5,1,'814185','2026-05-07 12:44:01',0),
(6,1,'538863','2026-05-07 13:12:35',0);
/*!40000 ALTER TABLE `login_otp` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `missing_person_report`
--

DROP TABLE IF EXISTS `missing_person_report`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `missing_person_report` (
  `Missing_Person_Report_ID` int NOT NULL,
  `Case_ID` int NOT NULL,
  `Report_ID` int NOT NULL,
  PRIMARY KEY (`Missing_Person_Report_ID`),
  KEY `Case_ID` (`Case_ID`),
  KEY `Report_ID` (`Report_ID`),
  CONSTRAINT `missing_person_report_ibfk_1` FOREIGN KEY (`Case_ID`) REFERENCES `cases` (`Case_ID`),
  CONSTRAINT `missing_person_report_ibfk_2` FOREIGN KEY (`Report_ID`) REFERENCES `incident_report` (`Report_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `missing_person_report`
--

LOCK TABLES `missing_person_report` WRITE;
/*!40000 ALTER TABLE `missing_person_report` DISABLE KEYS */;
/*!40000 ALTER TABLE `missing_person_report` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `notifications`
--

DROP TABLE IF EXISTS `notifications`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `notifications` (
  `id` int NOT NULL AUTO_INCREMENT,
  `title` varchar(200) DEFAULT NULL,
  `message` text NOT NULL,
  `type` enum('info','warning','alert','success') DEFAULT NULL,
  `related_table` varchar(50) DEFAULT NULL,
  `related_id` int DEFAULT NULL,
  `is_read` tinyint(1) DEFAULT '0',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `User_ID` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `notifications_user_FK` (`User_ID`),
  CONSTRAINT `notifications_user_FK` FOREIGN KEY (`User_ID`) REFERENCES `user` (`User_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `notifications`
--

LOCK TABLES `notifications` WRITE;
/*!40000 ALTER TABLE `notifications` DISABLE KEYS */;
/*!40000 ALTER TABLE `notifications` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `road_violation`
--

DROP TABLE IF EXISTS `road_violation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `road_violation` (
  `Violation_ID` int NOT NULL,
  `Violation_Type` varchar(20) NOT NULL,
  `Description` varchar(512) NOT NULL,
  `ViolationDate` date NOT NULL,
  `Location` varchar(255) NOT NULL,
  `Status` varchar(20) NOT NULL,
  `User_ID` int NOT NULL,
  `Vehicle_ID` int NOT NULL,
  PRIMARY KEY (`Violation_ID`),
  KEY `User_ID` (`User_ID`),
  KEY `Vehicle_ID` (`Vehicle_ID`),
  CONSTRAINT `road_violation_ibfk_1` FOREIGN KEY (`User_ID`) REFERENCES `user` (`User_ID`),
  CONSTRAINT `road_violation_ibfk_2` FOREIGN KEY (`Vehicle_ID`) REFERENCES `vehicles` (`Vehicle_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `road_violation`
--

LOCK TABLES `road_violation` WRITE;
/*!40000 ALTER TABLE `road_violation` DISABLE KEYS */;
/*!40000 ALTER TABLE `road_violation` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `roles`
--

DROP TABLE IF EXISTS `roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `roles` (
  `Role_ID` int NOT NULL,
  `Role_Name` varchar(20) NOT NULL,
  PRIMARY KEY (`Role_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `roles`
--

LOCK TABLES `roles` WRITE;
/*!40000 ALTER TABLE `roles` DISABLE KEYS */;
INSERT INTO `roles` VALUES
(1,'Admin');
/*!40000 ALTER TABLE `roles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `suspect_list`
--

DROP TABLE IF EXISTS `suspect_list`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `suspect_list` (
  `Suspect_ID` int NOT NULL,
  `Evidence` varchar(255) NOT NULL,
  `Investigation_ID` int NOT NULL,
  PRIMARY KEY (`Investigation_ID`,`Suspect_ID`),
  UNIQUE KEY `Suspect_ID` (`Suspect_ID`),
  CONSTRAINT `suspect_list_ibfk_1` FOREIGN KEY (`Investigation_ID`) REFERENCES `investigation_report` (`Investigation_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `suspect_list`
--

LOCK TABLES `suspect_list` WRITE;
/*!40000 ALTER TABLE `suspect_list` DISABLE KEYS */;
/*!40000 ALTER TABLE `suspect_list` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `system_logs`
--

DROP TABLE IF EXISTS `system_logs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `system_logs` (
  `Log_ID` int NOT NULL AUTO_INCREMENT,
  `User_ID` int NOT NULL,
  `Action` varchar(255) NOT NULL,
  `Table_Affected` varchar(50) DEFAULT NULL,
  `Created_At` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Log_ID`),
  KEY `User_ID` (`User_ID`),
  CONSTRAINT `system_logs_ibfk_1` FOREIGN KEY (`User_ID`) REFERENCES `user` (`User_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `system_logs`
--

LOCK TABLES `system_logs` WRITE;
/*!40000 ALTER TABLE `system_logs` DISABLE KEYS */;
/*!40000 ALTER TABLE `system_logs` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `test_arrests`
--

DROP TABLE IF EXISTS `test_arrests`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `test_arrests` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `test_arrests`
--

LOCK TABLES `test_arrests` WRITE;
/*!40000 ALTER TABLE `test_arrests` DISABLE KEYS */;
/*!40000 ALTER TABLE `test_arrests` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `traffic_accident`
--

DROP TABLE IF EXISTS `traffic_accident`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `traffic_accident` (
  `Accident_ID` int NOT NULL,
  `Location` varchar(255) NOT NULL,
  `Description` varchar(512) NOT NULL,
  `AccidentDate` date NOT NULL,
  `AccidentTime` varchar(20) NOT NULL,
  `Severity` int NOT NULL,
  `Status` varchar(20) NOT NULL,
  `User_ID` int NOT NULL,
  PRIMARY KEY (`Accident_ID`),
  KEY `User_ID` (`User_ID`),
  CONSTRAINT `traffic_accident_ibfk_1` FOREIGN KEY (`User_ID`) REFERENCES `user` (`User_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `traffic_accident`
--

LOCK TABLES `traffic_accident` WRITE;
/*!40000 ALTER TABLE `traffic_accident` DISABLE KEYS */;
/*!40000 ALTER TABLE `traffic_accident` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `traffic_fine`
--

DROP TABLE IF EXISTS `traffic_fine`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `traffic_fine` (
  `Fine_ID` int NOT NULL,
  `Amount` int NOT NULL,
  `IssueDate` date NOT NULL,
  `DueDate` date NOT NULL,
  `FineStatus` varchar(20) NOT NULL,
  `User_ID` int NOT NULL,
  `Violation_ID` int NOT NULL,
  PRIMARY KEY (`Fine_ID`),
  KEY `User_ID` (`User_ID`),
  KEY `Violation_ID` (`Violation_ID`),
  CONSTRAINT `traffic_fine_ibfk_1` FOREIGN KEY (`User_ID`) REFERENCES `user` (`User_ID`),
  CONSTRAINT `traffic_fine_ibfk_2` FOREIGN KEY (`Violation_ID`) REFERENCES `road_violation` (`Violation_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `traffic_fine`
--

LOCK TABLES `traffic_fine` WRITE;
/*!40000 ALTER TABLE `traffic_fine` DISABLE KEYS */;
/*!40000 ALTER TABLE `traffic_fine` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `traffic_report`
--

DROP TABLE IF EXISTS `traffic_report`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `traffic_report` (
  `Traffic_Report_ID` int NOT NULL,
  `User_ID` int NOT NULL,
  PRIMARY KEY (`Traffic_Report_ID`),
  KEY `User_ID` (`User_ID`),
  CONSTRAINT `traffic_report_ibfk_1` FOREIGN KEY (`User_ID`) REFERENCES `user` (`User_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `traffic_report`
--

LOCK TABLES `traffic_report` WRITE;
/*!40000 ALTER TABLE `traffic_report` DISABLE KEYS */;
/*!40000 ALTER TABLE `traffic_report` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `user`
--

DROP TABLE IF EXISTS `user`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `user` (
  `User_ID` int NOT NULL,
  `Name` varchar(20) NOT NULL,
  `Surname` varchar(20) NOT NULL,
  `Email` varchar(254) NOT NULL,
  `Password` varchar(255) NOT NULL,
  `Phone_Number` varchar(20) NOT NULL,
  `Address` varchar(254) NOT NULL,
  `Birth_Date` date NOT NULL,
  `ProfilePhoto` varchar(255) DEFAULT NULL,
  `Role_ID` int NOT NULL,
  `Department_ID` int NOT NULL,
  `Status` enum('active','suspended') DEFAULT 'active',
  PRIMARY KEY (`User_ID`),
  KEY `Role_ID` (`Role_ID`),
  KEY `Department_ID` (`Department_ID`),
  CONSTRAINT `user_ibfk_1` FOREIGN KEY (`Role_ID`) REFERENCES `roles` (`Role_ID`),
  CONSTRAINT `user_ibfk_2` FOREIGN KEY (`Department_ID`) REFERENCES `departments` (`Department_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user`
--

LOCK TABLES `user` WRITE;
/*!40000 ALTER TABLE `user` DISABLE KEYS */;
INSERT INTO `user` VALUES
(1,'john','doe','amarildowhack@gmail.com','$2a$12$dAuCOsmUIU289Gg38mgHUuS5QRaw2yzBM.fjvpDmWNceaG6zCz27i','0691234567','Tirana','2000-01-01',NULL,1,1,'active');
/*!40000 ALTER TABLE `user` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `vehicle_control`
--

DROP TABLE IF EXISTS `vehicle_control`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `vehicle_control` (
  `Control_ID` int NOT NULL,
  `User_ID` int NOT NULL,
  `Vehicle_ID` int NOT NULL,
  PRIMARY KEY (`Control_ID`),
  KEY `User_ID` (`User_ID`),
  KEY `Vehicle_ID` (`Vehicle_ID`),
  CONSTRAINT `vehicle_control_ibfk_1` FOREIGN KEY (`User_ID`) REFERENCES `user` (`User_ID`),
  CONSTRAINT `vehicle_control_ibfk_2` FOREIGN KEY (`Vehicle_ID`) REFERENCES `vehicles` (`Vehicle_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `vehicle_control`
--

LOCK TABLES `vehicle_control` WRITE;
/*!40000 ALTER TABLE `vehicle_control` DISABLE KEYS */;
/*!40000 ALTER TABLE `vehicle_control` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `vehicles`
--

DROP TABLE IF EXISTS `vehicles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `vehicles` (
  `Vehicle_ID` int NOT NULL,
  `PlateNumber` int NOT NULL,
  `Model` varchar(255) NOT NULL,
  `Brand` varchar(255) NOT NULL,
  `Color` varchar(255) NOT NULL,
  `RegistrationNumber` int NOT NULL,
  `RegistratiionStatus` varchar(20) NOT NULL,
  `User_ID` int NOT NULL,
  PRIMARY KEY (`Vehicle_ID`),
  KEY `User_ID` (`User_ID`),
  CONSTRAINT `vehicles_ibfk_1` FOREIGN KEY (`User_ID`) REFERENCES `user` (`User_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `vehicles`
--

LOCK TABLES `vehicles` WRITE;
/*!40000 ALTER TABLE `vehicles` DISABLE KEYS */;
/*!40000 ALTER TABLE `vehicles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `witness_list`
--

DROP TABLE IF EXISTS `witness_list`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `witness_list` (
  `Witness_ID` int NOT NULL,
  `Testimony` varchar(512) NOT NULL,
  `Investigation_ID` int NOT NULL,
  PRIMARY KEY (`Witness_ID`,`Investigation_ID`),
  KEY `Investigation_ID` (`Investigation_ID`),
  CONSTRAINT `witness_list_ibfk_1` FOREIGN KEY (`Investigation_ID`) REFERENCES `investigation_report` (`Investigation_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `witness_list`
--

LOCK TABLES `witness_list` WRITE;
/*!40000 ALTER TABLE `witness_list` DISABLE KEYS */;
/*!40000 ALTER TABLE `witness_list` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping routines for database 'policedb'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*M!100616 SET NOTE_VERBOSITY=@OLD_NOTE_VERBOSITY */;

-- Dump completed on 2026-05-12 20:55:15
