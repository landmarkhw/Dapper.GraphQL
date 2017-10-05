INSERT INTO Person (Id, FirstName, LastName) VALUES (1, 'Doug', 'Day');
INSERT INTO Person (Id, FirstName, LastName) VALUES (2, 'Kevin', 'Russon');

INSERT INTO Email (Id, [Address], PersonId) VALUES (1, 'dday@landmarkhw.com', 1);
INSERT INTO Email (Id, [Address], PersonId) VALUES (2, 'dougrday@gmail.com', 1);
INSERT INTO Email (Id, [Address], PersonId) VALUES (3, 'krusson@landmarkhw.com', 2);

INSERT INTO Phone (Id, Number, [Type], PersonId) VALUES (1, '8011234567', 3, 1);
INSERT INTO Phone (Id, Number, [Type], PersonId) VALUES (2, '8019876543', 3, 2);
INSERT INTO Phone (Id, Number, [Type], PersonId) VALUES (3, '8011111111', 1, 2);
