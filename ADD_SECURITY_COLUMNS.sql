-- ========================================
-- MIGRACIONES DE SEGURIDAD - PlataformaEscolar
-- ========================================

USE PlataformaEscolar;

-- Verificar y agregar FailedLoginAttempts
SET @col_exists_1 = 0;
SELECT COUNT(*) INTO @col_exists_1 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'PlataformaEscolar' 
  AND TABLE_NAME = 'Usuarios' 
  AND COLUMN_NAME = 'FailedLoginAttempts';

SET @query_1 = IF(@col_exists_1 = 0, 
    'ALTER TABLE Usuarios ADD COLUMN FailedLoginAttempts INT DEFAULT 0;',
    'SELECT ''? Columna FailedLoginAttempts ya existe'' AS Status;');

PREPARE stmt1 FROM @query_1;
EXECUTE stmt1;
DEALLOCATE PREPARE stmt1;

-- Verificar y agregar BloqueadoHasta
SET @col_exists_2 = 0;
SELECT COUNT(*) INTO @col_exists_2 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'PlataformaEscolar' 
  AND TABLE_NAME = 'Usuarios' 
  AND COLUMN_NAME = 'BloqueadoHasta';

SET @query_2 = IF(@col_exists_2 = 0, 
    'ALTER TABLE Usuarios ADD COLUMN BloqueadoHasta DATETIME NULL;',
    'SELECT ''? Columna BloqueadoHasta ya existe'' AS Status;');

PREPARE stmt2 FROM @query_2;
EXECUTE stmt2;
DEALLOCATE PREPARE stmt2;

-- Mostrar resultado
SELECT '? Migraciones completadas' AS Resultado;
