# DBMProject

Automate deployment of database changes


# Instruction

Database Simple Schema

```

CREATE TABLE [dbo].[version](
	
[id] [int] IDENTITY(1,1) NOT NULL,
	
[script_name] [varchar](50) NULL,
	
[applied_date] [datetime] NULL
) 
ON [PRIMARY]

```
