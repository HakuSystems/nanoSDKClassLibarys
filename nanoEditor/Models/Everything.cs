﻿using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace nanoEditor.Models;

public class Everything : MonoBehaviour
{
    private const int EVERYTHING_REQUEST_FILE_NAME = 0x00000001;
    private const int EVERYTHING_REQUEST_PATH = 0x00000002;
    private const int EVERYTHING_REQUEST_SIZE = 0x00000010;


    [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
    public static extern uint Everything_SetSearchW(string lpSearchString);

    [DllImport("Everything64.dll")]
    public static extern void Everything_SetMatchPath(bool bEnable);

    [DllImport("Everything64.dll")]
    public static extern void Everything_SetMatchCase(bool bEnable);

    [DllImport("Everything64.dll")]
    public static extern void Everything_SetMatchWholeWord(bool bEnable);

    [DllImport("Everything64.dll")]
    public static extern void Everything_SetRegex(bool bEnable);

    [DllImport("Everything64.dll")]
    public static extern void Everything_SetMax(uint dwMax);

    [DllImport("Everything64.dll")]
    public static extern void Everything_SetOffset(uint dwOffset);

    [DllImport("Everything64.dll")]
    public static extern bool Everything_GetMatchPath();

    [DllImport("Everything64.dll")]
    public static extern bool Everything_GetMatchCase();

    [DllImport("Everything64.dll")]
    public static extern bool Everything_GetMatchWholeWord();

    [DllImport("Everything64.dll")]
    public static extern bool Everything_GetRegex();

    [DllImport("Everything64.dll")]
    public static extern uint Everything_GetMax();

    [DllImport("Everything64.dll")]
    public static extern uint Everything_GetOffset();

    [DllImport("Everything64.dll")]
    public static extern IntPtr Everything_GetSearchW();

    [DllImport("Everything64.dll")]
    public static extern uint Everything_GetLastError();

    [DllImport("Everything64.dll")]
    public static extern bool Everything_QueryW(bool bWait);

    [DllImport("Everything64.dll")]
    public static extern void Everything_SortResultsByPath();

    [DllImport("Everything64.dll")]
    public static extern uint Everything_GetNumFileResults();

    [DllImport("Everything64.dll")]
    public static extern uint Everything_GetNumFolderResults();

    [DllImport("Everything64.dll")]
    public static extern uint Everything_GetNumResults();

    [DllImport("Everything64.dll")]
    public static extern uint Everything_GetTotFileResults();

    [DllImport("Everything64.dll")]
    public static extern uint Everything_GetTotFolderResults();

    [DllImport("Everything64.dll")]
    public static extern uint Everything_GetTotResults();

    [DllImport("Everything64.dll")]
    public static extern bool Everything_IsVolumeResult(uint nIndex);

    [DllImport("Everything64.dll")]
    public static extern bool Everything_IsFolderResult(uint nIndex);

    [DllImport("Everything64.dll")]
    public static extern bool Everything_IsFileResult(uint nIndex);

    [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
    public static extern void Everything_GetResultFullPathName(uint nIndex, StringBuilder lpString, uint nMaxCount);

    [DllImport("Everything64.dll")]
    public static extern void Everything_Reset();

    [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr Everything_GetResultFileName(uint nIndex);

    [DllImport("Everything64.dll")]
    public static extern void Everything_SetSort(uint dwSortType);

    [DllImport("Everything64.dll")]
    public static extern uint Everything_GetSort();

    [DllImport("Everything64.dll")]
    public static extern uint Everything_GetResultListSort();

    [DllImport("Everything64.dll")]
    public static extern void Everything_SetRequestFlags(uint dwRequestFlags);

    [DllImport("Everything64.dll")]
    public static extern uint Everything_GetRequestFlags();

    [DllImport("Everything64.dll")]
    public static extern uint Everything_GetResultListRequestFlags();

    [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr Everything_GetResultExtension(uint nIndex);

    [DllImport("Everything64.dll")]
    public static extern bool Everything_GetResultSize(uint nIndex, out long lpFileSize);

    [DllImport("Everything64.dll")]
    public static extern bool Everything_GetResultDateCreated(uint nIndex, out long lpFileTime);

    [DllImport("Everything64.dll")]
    public static extern bool Everything_GetResultDateModified(uint nIndex, out long lpFileTime);

    [DllImport("Everything64.dll")]
    public static extern bool Everything_GetResultDateAccessed(uint nIndex, out long lpFileTime);

    [DllImport("Everything64.dll")]
    public static extern uint Everything_GetResultAttributes(uint nIndex);

    [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr Everything_GetResultFileListFileName(uint nIndex);

    [DllImport("Everything64.dll")]
    public static extern uint Everything_GetResultRunCount(uint nIndex);

    [DllImport("Everything64.dll")]
    public static extern bool Everything_GetResultDateRun(uint nIndex, out long lpFileTime);

    [DllImport("Everything64.dll")]
    public static extern bool Everything_GetResultDateRecentlyChanged(uint nIndex, out long lpFileTime);

    [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr Everything_GetResultHighlightedFileName(uint nIndex);

    [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr Everything_GetResultHighlightedPath(uint nIndex);

    [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr Everything_GetResultHighlightedFullPathAndFileName(uint nIndex);

    [DllImport("Everything64.dll")]
    public static extern uint Everything_GetRunCountFromFileName(string lpFileName);

    [DllImport("Everything64.dll")]
    public static extern bool Everything_SetRunCountFromFileName(string lpFileName, uint dwRunCount);

    [DllImport("Everything64.dll")]
    public static extern uint Everything_IncRunCountFromFileName(string lpFileName);


    public static IEnumerable<Result> Search(string qry, int maxResults = 200)
    {
        // set the search            
        Everything_SetSearchW(qry);
        Everything_SetRequestFlags(EVERYTHING_REQUEST_FILE_NAME | EVERYTHING_REQUEST_SIZE | EVERYTHING_REQUEST_PATH);

        // execute the query
        Everything_QueryW(true);
        var resultCount = Everything_GetNumResults();
        var low = Math.Min(resultCount, maxResults);
        // Debug.Log(low);
        for (uint i = 0; i < low; i++)
        {
            var sb = new StringBuilder(999);

            Everything_GetResultFullPathName(i, sb, 999);
            Everything_GetResultSize(i, out var size);

            EditorUtility.DisplayProgressBar("Searching", sb.ToString(), i / (float)low);
            // Debug.Log(sb.ToString());

            yield return new Result
            {
                Filename = Marshal.PtrToStringUni(Everything_GetResultFileName(i)),
                Size = size,
                Path = sb.ToString()
            };
        }

        EditorUtility.ClearProgressBar();
    }


    public struct Result
    {
        public long Size;
        public string Filename;
        public string Path;

        public bool Folder => Size < 0;

        public string ResultString()
        {
            return Filename;
        }
    }
}