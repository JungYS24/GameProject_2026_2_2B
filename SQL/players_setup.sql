-- 1. 테이블 생성
CREATE TABLE `players` (
    `player_id` INT NOT NULL AUTO_INCREMENT,
    `username` VARCHAR(50) NOT NULL DEFAULT '0',
    `email` VARCHAR(50) NOT NULL,
    `password_hash` VARCHAR(255) NOT NULL,
    `created_at` TIMESTAMP NOT NULL DEFAULT (CURRENT_TIMESTAMP()),    
    `last_login` TIMESTAMP NULL DEFAULT NULL,
    PRIMARY KEY (`player_id`),
    UNIQUE INDEX `username` (`username`),
    UNIQUE INDEX `email` (`email`)
)
COLLATE='utf8mb4_0900_ai_ci';


-- 2. 플레이어 데이터 삽입
INSERT INTO players (username, email, password_hash) VALUES ('hero123', 'hero123@gmail.com', 'hashed_password1');
SELECT * FROM `gametest`.`players` LIMIT 1000;

-- 3. 여러 개 플레이어 데이터 삽입
INSERT INTO players (username, email, password_hash) VALUES
('hero423', 'hero323@gmail.com', 'hashed_password3'),
('hero523', 'hero423@gmail.com', 'hashed_password4'),
('hero623', 'hero523@gmail.com', 'hashed_password5'),
('hero723', 'hero623@gmail.com', 'hashed_password6'),
('hero823', 'hero723@gmail.com', 'hashed_password7');

-- 4. 유저 이름 기준으로 최근 로그인 보기
SELECT username, LAST+login FROM players

-- 5. 특정 플레이어 정보 업데이트
UPDATE players SET last_login = CURRENT_TIMESTAMP WHERE username = 'hero123'

-- 6. 조건에 맞는 플레이어 검색
SELECT username, email FROM players WHERE username LIKE '%hero%'

-- 7. 플레이어 삭제
DELETE FROM players WHERE username = 'hero123'