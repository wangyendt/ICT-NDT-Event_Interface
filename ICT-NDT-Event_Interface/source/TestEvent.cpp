// TestEvent.cpp : 定义应用程序的入口点。
//

#include "stdafx.h"
#include "TestEvent.h"
#include <atlbase.h>

#pragma warning(disable: 4996)

#define		STR_equ		" = "

int		WChar2Char(WCHAR *pSor, CHAR *pDest, int nDestLen, UINT uCodePage = CP_ACP)
{
	int		i, nRetVal;

	if(nDestLen == -1)
		nDestLen = ::WideCharToMultiByte(uCodePage, 0, pSor, -1, NULL, 0, NULL, NULL);
	else
		nDestLen--;

	for(i=0; (nDestLen-i)&&i<3; i++)
		pDest[nDestLen-i-1] = 0;
	nRetVal = ::WideCharToMultiByte(uCodePage, 0, pSor, -1, pDest, nDestLen, NULL, NULL);
	return nRetVal;
}


int		TChar2Char(TCHAR *pSor, CHAR *pDest, int nDestLen = 1024, UINT uCodePage = CP_ACP)
{
#ifndef UNICODE
#ifndef _UNICODE
	return strcpy_s(pDest, nDestLen, pSor);
#endif
#endif

	return WChar2Char((WCHAR *)pSor, pDest, nDestLen, uCodePage);
}


BYTE	* GetFileData(CHAR *pFileName, DWORD * pdwSize)
{
	BY_HANDLE_FILE_INFORMATION	HFileInfo;
	HANDLE						hFile;
	BOOL						bRetVal;
	DWORD						dwSize, dwRead;
	CHAR						* MainBuf;

	if(pdwSize)
		*pdwSize = 0;
	hFile = CreateFileA(pFileName, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE, NULL, OPEN_EXISTING, 0, NULL);
	if(hFile == INVALID_HANDLE_VALUE)
	{
		return NULL;
	}

	bRetVal = GetFileInformationByHandle(hFile, &HFileInfo);
	if(bRetVal == FALSE)
	{
		CloseHandle(hFile);
		return NULL;
	}

	if(HFileInfo.nFileSizeHigh != 0)
	{
		CloseHandle(hFile);
		return NULL;
	}

	dwSize = HFileInfo.nFileSizeLow;
	if(pdwSize)
		*pdwSize = dwSize;

	MainBuf = (CHAR *)malloc(dwSize+2);
	if(MainBuf == NULL)
	{
		CloseHandle(hFile);
		return FALSE;
	}

	bRetVal = ::ReadFile(hFile, MainBuf, dwSize, &dwRead, NULL);
	if(bRetVal == FALSE || dwRead != dwSize)
	{
		CloseHandle(hFile);
		free(MainBuf);
		return NULL;
	}

	MainBuf[dwSize] = 0;
	MainBuf[dwSize+1] = 0;
	CloseHandle(hFile);
	return (BYTE *)MainBuf;
}

DWORD	DumpToFile(CHAR *pFileName, VOID *pMem, DWORD dwSize)
{
	HANDLE			hFile;
	DWORD			dwWrite;
	BOOL			bRetVal;

	if(pMem == NULL)
		return 0;
	hFile = CreateFileA(pFileName, GENERIC_WRITE | GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
	if(hFile == INVALID_HANDLE_VALUE)
		return 0;

	if(dwSize == 0)
		dwSize = (DWORD)strlen((CONST CHAR *)pMem);

	dwWrite = 0;
	bRetVal = ::WriteFile(hFile, pMem, dwSize, &dwWrite, NULL);
	CloseHandle(hFile);

	return dwWrite;
}


int		GetLine(CHAR **ppLastPos, CHAR *pBuffer, CHAR *pOneLine = NULL, int nLineMaxSize = 1024)
{
	static  CHAR	* pCur = NULL;
	int				i = 0;
	int				nRetVal;

	if(ppLastPos == NULL)
		ppLastPos = &pCur;
	if(pOneLine == NULL)		//初始化
	{
		(*ppLastPos) = pBuffer;
		return 1;
	}
	if((*ppLastPos) == NULL)			//没有初始化
		return 0;
	if((*ppLastPos)[0] == 0)
		return 0;

	nRetVal = 0;
	for(i=0; ; i++)
	{
		if(((*ppLastPos))[i] == 0x0D && (*ppLastPos)[i+1] == 0x0A)
		{
			(*ppLastPos) += i+2;
			nRetVal = 1;
			break;
		}
		if((*ppLastPos)[i] == 0x0D)
		{
			(*ppLastPos) += i+1;
			nRetVal = 1;
			break;
		}
		if((*ppLastPos)[i] == 0x0A)
		{
			(*ppLastPos) += i+1;
			nRetVal = 1;
			break;
		}
		if((*ppLastPos)[i] == 0)
		{
			(*ppLastPos) += i;
			nRetVal = 2;
			break;
		}
		if(i < nLineMaxSize)
			pOneLine[i] = (*ppLastPos)[i];
	}

	if(i < nLineMaxSize)
		pOneLine[i] = 0;
	else
		pOneLine[nLineMaxSize-1] = 0;

	return nRetVal;
}

BOOL	TextDecode(CHAR *pText)
{
	CHAR		szTemp[1024], szResult[1024];
	int			i, j;

	strcpy(szTemp, pText);
	j = 0;
	for(i=0; szTemp[i]; i++)
	{
		if(szTemp[i] != '\\')
		{
			szResult[j] = szTemp[i];
			j ++;
			continue;
		}
		switch(szTemp[i+1])
		{
		case '\\':		szResult[j] = '\\';		j++;		break;
		case 'r':		szResult[j] = '\r';		j++;		break;
		case 'R':		szResult[j] = '\r';		j++;		break;
		case 'n':		szResult[j] = '\n';		j++;		break;
		case 'N':		szResult[j] = '\n';		j++;		break;
		default:		
			szResult[j] = '\\';				j++;		
			szResult[j] = szTemp[i+1];		j++;		
		}
		i ++;
	}
	szResult[j] = 0;
	strcpy(pText, szResult);
	return TRUE;
}



BOOL	ReadInputInfo(CHAR *pFilePath, map<string, string> &Info)
{
	CHAR		*pText, *pLastPos, *pFind;
	DWORD		dwSize;
	CHAR		szLine[1024], szName[256], szValue[1024];


	pText = (CHAR *)GetFileData(pFilePath, &dwSize);
	if(pText == NULL)
		return FALSE;

	for(GetLine(&pLastPos, pText);
		GetLine(&pLastPos, pText, szLine, sizeof(szLine));
		)
	{
		if(szLine[0] == '/')
			continue;

		pFind = strstr(szLine, STR_equ);
		if(pFind == NULL)
			continue;

		pFind[0] = 0;
		strcpy(szName, szLine);
		pFind += strlen(STR_equ);
		strcpy(szValue, pFind);
		TextDecode(szValue);

		Info[szName] = szValue;
	}
	return TRUE;
}


int APIENTRY _tWinMain(HINSTANCE hInstance,
                     HINSTANCE hPrevInstance,
                     LPTSTR    lpCmdLine,
                     int       nCmdShow)
{
	map<string, string>		InputInfo;
	CHAR		szInputFile[MAX_PATH], szFilePath[MAX_PATH];
	BOOL		bRetVal;
	CHAR		szEvent[64];

	if(lpCmdLine == NULL)
		return 1;

#ifdef _UNICODE
	TChar2Char(__wargv[1], szInputFile, sizeof(szInputFile));
#else
	strcpy(szInputFile, __argv[1]);
#endif


	bRetVal = ReadInputInfo(szInputFile, InputInfo);
	if(bRetVal == FALSE)
		return 2;

	strcpy(szEvent, InputInfo["Event"].c_str());
	if(stricmp(szEvent, "TestStart") == 0)
	{
		EventTestStart(InputInfo);
		return 0;
	}

	if(stricmp(szEvent, "TestDone") == 0)
	{
		EventTestDone(InputInfo);
		return 0;
	}

	if(stricmp(szEvent, "TestResult") == 0)
	{
		EventTestResult(InputInfo);
		return 0;
	}

	return 0;
}


//		测试开始的事件, 在此检查输入的条码等
BOOL	EventTestStart(map<string, string> &Info)
{
	CHAR		szResultFile[MAX_PATH];
	CHAR		szText[1024], szBoard[64], szBarcode[64];
	int			i, nCount;
	BOOL		bTestCancel;

	bTestCancel = FALSE;
	strcpy(szText, Info["BoardCount"].c_str());
	nCount = atoi(szText);
	for(i=0; i<nCount; i++)
	{
		sprintf(szBoard, "B%d.Barcode", i+1);
		strcpy(szBarcode, Info[szBoard].c_str());

		//	在此检查条码
		ATLTRACE("Barcode: %2d - %s\r\n", i+1, szBarcode);
	}

	strcpy(szResultFile, Info["Result"].c_str());
	WriteResult(szResultFile, bTestCancel, "");

	return TRUE;
}

//		测试完成的事件, 但是详细结果尚未出来
BOOL	EventTestDone(map<string, string> &Info)
{
	CHAR		szResultFile[MAX_PATH], szBoard[64], szText[64], szBarcode[64];
	BOOL		bTestCancel, bPass;
	int			i, nCount;

	bTestCancel = FALSE;
	strcpy(szText, Info["BoardCount"].c_str());
	nCount = atoi(szText);

	for(i=0; i<nCount; i++)
	{
		sprintf(szBoard, "B%d.Barcode", i+1);
		strcpy(szBarcode, Info[szBoard].c_str());

		sprintf(szBoard, "B%d.Pass", i+1);
		strcpy(szText, Info[szBoard].c_str());
		bPass = atoi(szText);

		//	
		ATLTRACE("PASS: %2d - %d - %s\r\n", i+1, bPass, szBarcode);
	}

	strcpy(szResultFile, Info["Result"].c_str());
	WriteResult(szResultFile, bTestCancel, "");

	return TRUE;
}

//		测试完成后已生成详细的测试日志
BOOL	EventTestResult(map<string, string> &Info)
{
	CHAR		szResultFile[MAX_PATH], szName[64], szText[1024];
	BOOL		bTestCancel;
	int			i;

	bTestCancel = FALSE;
	for(i=0; ; i++)
	{
		sprintf(szName, "Detail%d", i+1);
		strcpy(szText, Info[szName].c_str());
		if(szText[0] == 0)
			break;

		//	
		ATLTRACE("TEST Detail: %2d - %s\r\n", i+1, szText);
	}

	strcpy(szResultFile, Info["Result"].c_str());
	WriteResult(szResultFile, bTestCancel, "");

	return TRUE;
}


//	pFilePath: 要写入的文件
//	bTestCancel: 是否需要取消测试
//	pMsgBox: 弹出提示用户的文字信息
BOOL	WriteResult(CHAR *pFilePath, BOOL bTestCancel, CHAR *pMsgBox)
{
	CHAR		szText[4096];
	DWORD		dwWrite;

	sprintf(szText, 
		"TestCancel = %d\r\n"
		"InfoText = %s\r\n", bTestCancel, pMsgBox);

	dwWrite = DumpToFile(pFilePath, szText, strlen(szText));

	return (dwWrite != 0);
}
