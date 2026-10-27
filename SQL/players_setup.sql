USE `gametest`;

-- 기존 테이블 전체 정리 (자식 테이블 -> 부모 테이블 순서로 삭제)
DROP TABLE IF EXISTS `player_quests`;
DROP TABLE IF EXISTS `inventories`;
DROP TABLE IF EXISTS `quests`;
DROP TABLE IF EXISTS `items`;
DROP TABLE IF EXISTS `players`;

-- 1. players 테이블 생성
CREATE TABLE `players` (
    `player_id` INT NOT NULL AUTO_INCREMENT,
    `username` VARCHAR(50) NOT NULL DEFAULT '0',
    `email` VARCHAR(50) NOT NULL,
    `password_hash` VARCHAR(255) NOT NULL,
    `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,    
    `last_login` TIMESTAMP NULL DEFAULT NULL,
    PRIMARY KEY (`player_id`),
    UNIQUE INDEX `username` (`username`),
    UNIQUE INDEX `email` (`email`)
) COLLATE='utf8mb4_0900_ai_ci';

-- 2. 단일 플레이어 데이터 삽입 (player_id = 1)
INSERT INTO `players` (`username`, `email`, `password_hash`) 
VALUES ('hero123', 'hero123@gmail.com', 'hashed_password1');

SELECT * FROM `players` LIMIT 1000;

-- 3. 여러 플레이어 데이터 삽입 (player_id = 2, 3, 4, 5, 6)
INSERT INTO `players` (`username`, `email`, `password_hash`) VALUES
('hero423', 'hero323@gmail.com', 'hashed_password3'),
('hero523', 'hero423@gmail.com', 'hashed_password4'),
('hero623', 'hero523@gmail.com', 'hashed_password5'),
('hero723', 'hero623@gmail.com', 'hashed_password6'),
('hero823', 'hero723@gmail.com', 'hashed_password7');

-- 4. 유저 이름 기준으로 최근 로그인 보기
SELECT `username`, `last_login` FROM `players`;

-- 5. 특정 플레이어 정보 업데이트
UPDATE `players` SET `last_login` = CURRENT_TIMESTAMP WHERE `username` = 'hero123';

-- 6. 조건에 맞는 플레이어 검색
SELECT `username`, `email` FROM `players` WHERE `username` LIKE '%hero%';

-- 7. 플레이어 삭제 실습
DELETE FROM `players` WHERE `username` = 'hero123';

SELECT * FROM `players` LIMIT 1000;

-- 8. 플레이어 테이블에 새 열 추가
ALTER TABLE `players` ADD COLUMN `level` INT DEFAULT 1;

-- 9. 모든 플레이어의 레벨 1 증가
UPDATE `players` SET `level` = `level` + 1;

-- 10. 가장 LEVEL이 높은 플레이어 조회
SELECT `username`, `level` FROM `players` ORDER BY `level` DESC LIMIT 1;


-- 11. items 테이블 생성
CREATE TABLE `items` (
    `item_id` INT AUTO_INCREMENT PRIMARY KEY,
    `name` VARCHAR(100) NOT NULL,
    `description` TEXT,
    `value` INT DEFAULT 0
);

-- 12. 아이템 데이터 삽입 (item_id = 1, 2, 3)
INSERT INTO `items` (`name`, `description`, `value`) VALUES
('검', '기본 무기', 10),
('방패', '기본 방어구', 15),
('물약', '체력을 회복', 5);

SELECT * FROM `items`;

-- 13. 새로운 아이템 추가 (item_id = 4)
INSERT INTO `items` (`name`, `description`, `value`) VALUES
('용검', '용 기본 무기', 20);

-- 14. 가장 가치 있는 아이템 찾기
SELECT `name`, `value` FROM `items` ORDER BY `value` DESC LIMIT 1;


-- 15. inventories 테이블 생성
CREATE TABLE `inventories` (
    `inventory_id` INT AUTO_INCREMENT PRIMARY KEY,
    `player_id` INT,
    `item_id` INT,
    `quantity` INT DEFAULT 1,
    FOREIGN KEY (`player_id`) REFERENCES `players`(`player_id`),
    FOREIGN KEY (`item_id`) REFERENCES `items`(`item_id`)
);

-- 16. 인벤토리에 아이템 추가
INSERT INTO `inventories` (`player_id`, `item_id`, `quantity`) VALUES
(6, 1, 1),
(4, 2, 5),
(5, 3, 2),
(5, 4, 2);

-- 17. 플레이어의 인벤토리 조회 (JOIN)
SELECT p.`username`, i.`name`, inv.`quantity`
FROM `players` p 
JOIN `inventories` inv ON p.`player_id` = inv.`player_id`
JOIN `items` i ON inv.`item_id` = i.`item_id`;


-- 18. quests 테이블 생성
CREATE TABLE `quests` (
    `quest_id` INT AUTO_INCREMENT PRIMARY KEY,
    `title` VARCHAR(100) NOT NULL,
    `description` TEXT,
    `reward_exp` INT DEFAULT 0,
    `reward_item_id` INT,
    FOREIGN KEY (`reward_item_id`) REFERENCES `items`(`item_id`)
);

-- 19. 퀘스트 데이터 삽입 (quest_id = 1, 2)
INSERT INTO `quests` (`title`, `description`, `reward_exp`, `reward_item_id`) VALUES
('초보자 퀘스트', '첫 번째 퀘스트를 완료 하세요', 100, 3),
('용사의 검', '전설의 검을 찾아보세요', 500, 1);


-- 20. player_quests 테이블 생성
CREATE TABLE `player_quests` (
    `player_id` INT,
    `quest_id` INT,
    `status` ENUM('시작', '진행중', '완료') DEFAULT '시작',
    `start_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP(),
    `completed_at` TIMESTAMP NULL,
    PRIMARY KEY (`player_id`, `quest_id`),
    FOREIGN KEY (`player_id`) REFERENCES `players`(`player_id`),
    FOREIGN KEY (`quest_id`) REFERENCES `quests`(`quest_id`)
);

-- 21. 플레이어에게 퀘스트 할당
INSERT INTO `player_quests` (`player_id`, `quest_id`) VALUES
(4, 1),
(5, 2);

-- 22. 진행 중인 퀘스트 조회
SELECT p.`username`, q.`title`, pq.`status`
FROM `players` p
JOIN `player_quests` pq ON p.`player_id` = pq.`player_id`
JOIN `quests` q ON pq.`quest_id` = q.`quest_id`
WHERE pq.`status` != '완료';
