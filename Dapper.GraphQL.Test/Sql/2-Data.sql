SET IDENTITY_INSERT Person ON
INSERT INTO Person (Id, MergedToPersonId, FirstName, LastName, SupervisorId, CareerCounselorId) VALUES (1, 1, 'Hyrum', 'Clyde', NULL, NULL);
INSERT INTO Person (Id, MergedToPersonId, FirstName, LastName, SupervisorId, CareerCounselorId) VALUES (2, 2, 'Doug', 'Day', NULL, NULL);
INSERT INTO Person (Id, MergedToPersonId, FirstName, LastName, SupervisorId, CareerCounselorId) VALUES (3, 3, 'Kevin', 'Russon', 1, 2);
INSERT INTO Person (Id, MergedToPersonId, FirstName, LastName, SupervisorId, CareerCounselorId) VALUES (4, 4, 'Douglas', 'Day', NULL, 1);
SET IDENTITY_INSERT Person OFF

-- Merge people (Doug == Douglas)
UPDATE Person
SET MergedToPersonId = 4
WHERE Id = 2;

SET IDENTITY_INSERT Company ON
INSERT INTO Company (Id, Name) VALUES (1, 'Landmark Home Warranty, LLC');
INSERT INTO Company (Id, Name) VALUES (2, 'Navitaire, LLC');
SET IDENTITY_INSERT Company OFF

SET IDENTITY_INSERT PersonCompany ON
INSERT INTO PersonCompany (Id, PersonId, CompanyId, StartDate, EndDate) VALUES (1, 1, 1, '2016-01-01', NULL);
INSERT INTO PersonCompany (Id, PersonId, CompanyId, StartDate, EndDate) VALUES (2, 2, 1, '2016-05-16', NULL);
INSERT INTO PersonCompany (Id, PersonId, CompanyId, StartDate, EndDate) VALUES (3, 3, 1, '2016-10-05', NULL);
INSERT INTO PersonCompany (Id, PersonId, CompanyId, StartDate, EndDate) VALUES (4, 4, 2, '2011-04-06', '2016-05-13');
INSERT INTO PersonCompany (Id, PersonId, CompanyId, StartDate, EndDate) VALUES (4, 3, 2, '2011-08-15', '2016-10-02');
SET IDENTITY_INSERT PersonCompany OFF

SET IDENTITY_INSERT Email ON
INSERT INTO Email (Id, [Address]) VALUES (1, 'hclyde@landmarkhw.com');
INSERT INTO Email (Id, [Address]) VALUES (2, 'dday@landmarkhw.com');
INSERT INTO Email (Id, [Address]) VALUES (3, 'dougrday@gmail.com');
INSERT INTO Email (Id, [Address]) VALUES (4, 'krusson@landmarkhw.com');
INSERT INTO Email (Id, [Address]) VALUES (5, 'whole-company@landmarkhw.com');
INSERT INTO Email (Id, [Address]) VALUES (6, 'whole-company@navitaire.com');
SET IDENTITY_INSERT Email OFF

SET IDENTITY_INSERT PersonEmail ON
INSERT INTO PersonEmail (Id, EmailId, PersonId) VALUES (1, 1, 1);
INSERT INTO PersonEmail (Id, EmailId, PersonId) VALUES (2, 2, 2);
INSERT INTO PersonEmail (Id, EmailId, PersonId) VALUES (3, 3, 4);
INSERT INTO PersonEmail (Id, EmailId, PersonId) VALUES (4, 4, 3);
SET IDENTITY_INSERT PersonEmail OFF

SET IDENTITY_INSERT CompanyEmail ON
INSERT INTO CompanyEmail (Id, CompanyId, EmailId) VALUES (1, 1, 5);
INSERT INTO CompanyEmail (Id, CompanyId, EmailId) VALUES (2, 2, 6);
SET IDENTITY_INSERT CompanyEmail OFF

SET IDENTITY_INSERT Phone ON
INSERT INTO Phone (Id, Number, [Type]) VALUES (1, '8011234567', 3);
INSERT INTO Phone (Id, Number, [Type]) VALUES (2, '8019876543', 3);
INSERT INTO Phone (Id, Number, [Type]) VALUES (3, '8011111111', 1);
INSERT INTO Phone (Id, Number, [Type]) VALUES (4, '8663062999', 1);
INSERT INTO Phone (Id, Number, [Type]) VALUES (5, '8019477800', 1);
SET IDENTITY_INSERT Phone OFF

SET IDENTITY_INSERT PersonPhone ON
INSERT INTO PersonPhone (Id, PhoneId, PersonId) VALUES (1, 1, 2);
INSERT INTO PersonPhone (Id, PhoneId, PersonId) VALUES (2, 2, 3);
INSERT INTO PersonPhone (Id, PhoneId, PersonId) VALUES (3, 3, 3);
SET IDENTITY_INSERT PersonPhone OFF

SET IDENTITY_INSERT CompanyPhone ON
INSERT INTO CompanyPhone (Id, PhoneId, CompanyId) VALUES (1, 4, 1);
INSERT INTO CompanyPhone (Id, PhoneId, CompanyId) VALUES (2, 5, 2);
SET IDENTITY_INSERT CompanyPhone OFF