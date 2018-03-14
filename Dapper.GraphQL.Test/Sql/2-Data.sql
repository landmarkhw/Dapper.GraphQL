INSERT INTO Person (Id, FirstName, LastName, SupervisorId, CareerCounselorId) VALUES (1, 'Hyrum', 'Clyde', NULL, NULL);
INSERT INTO Person (Id, FirstName, LastName, SupervisorId, CareerCounselorId) VALUES (2, 'Doug', 'Day', NULL, 1);
INSERT INTO Person (Id, FirstName, LastName, SupervisorId, CareerCounselorId) VALUES (3, 'Kevin', 'Russon', 1, 2);

-- Add the relationship once all records are inserted
--UPDATE Person 
--SET CareerCounselorId = 2
--WHERE Id = 1;

INSERT INTO Email (Id, [Address], PersonId) VALUES (1, 'hclyde@landmarkhw.com', 1);
INSERT INTO Email (Id, [Address], PersonId) VALUES (2, 'dday@landmarkhw.com', 2);
INSERT INTO Email (Id, [Address], PersonId) VALUES (3, 'dougrday@gmail.com', 2);
INSERT INTO Email (Id, [Address], PersonId) VALUES (4, 'krusson@landmarkhw.com', 3);

INSERT INTO Phone (Id, Number, [Type], PersonId) VALUES (1, '8011234567', 3, 2);
INSERT INTO Phone (Id, Number, [Type], PersonId) VALUES (2, '8019876543', 3, 3);
INSERT INTO Phone (Id, Number, [Type], PersonId) VALUES (3, '8011111111', 1, 3);
