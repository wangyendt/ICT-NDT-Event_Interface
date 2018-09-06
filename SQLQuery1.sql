SELECT TOP 100 * FROM Test_2 WHERE N_BarCode = 'barcode_data4'

INSERT INTO  Test_2
           (N_BX
           ,N_DeviceName
           ,N_TestVal
           ,N_Result
           ,N_DateTime
           ,N_BarCode
           ,N_BoardName
           ,N_OpenTest
           ,N_ShortTest
           ,N_VoltageResult
           ,N_ResistanceResult
           ,N_MachineID
           ,N_Operator
           ,N_TestType)
     VALUES
           ('B1','��V3-CH3',-0.057,'Pass','20180906 05:11:45','asdf4','ADC','Pass','Pass','Pass','Pass','1234','James','V')


DELETE dbo.Test_2
