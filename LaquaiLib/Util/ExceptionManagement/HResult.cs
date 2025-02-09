﻿namespace LaquaiLib.Util.ExceptionManagement;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Defines constants for some common HResults.
/// Stolen straight from the .NET runtime source (see <see href="https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/System/HResults.cs"/>.
/// </summary>
public static partial class HResults
{
    public const int S_OK = unchecked(0);
    public const int S_FALSE = unchecked(1);
    public const int COR_E_ABANDONEDMUTEX = unchecked((int)0x8013152D);
    public const int COR_E_AMBIGUOUSIMPLEMENTATION = unchecked((int)0x8013106A);
    public const int COR_E_AMBIGUOUSMATCH = unchecked((int)0x8000211D);
    public const int COR_E_APPDOMAINUNLOADED = unchecked((int)0x80131014);
    public const int COR_E_APPLICATION = unchecked((int)0x80131600);
    public const int COR_E_ARGUMENT = unchecked((int)0x80070057);
    public const int COR_E_ARGUMENTOUTOFRANGE = unchecked((int)0x80131502);
    public const int COR_E_ARITHMETIC = unchecked((int)0x80070216);
    public const int COR_E_ARRAYTYPEMISMATCH = unchecked((int)0x80131503);
    public const int COR_E_BADEXEFORMAT = unchecked((int)0x800700C1);
    public const int COR_E_BADIMAGEFORMAT = unchecked((int)0x8007000B);
    public const int COR_E_CANNOTUNLOADAPPDOMAIN = unchecked((int)0x80131015);
    public const int COR_E_CODECONTRACTFAILED = unchecked((int)0x80131542);
    public const int COR_E_CONTEXTMARSHAL = unchecked((int)0x80131504);
    public const int COR_E_CUSTOMATTRIBUTEFORMAT = unchecked((int)0x80131605);
    public const int COR_E_DATAMISALIGNED = unchecked((int)0x80131541);
    public const int COR_E_DIRECTORYNOTFOUND = unchecked((int)0x80070003);
    public const int COR_E_DIVIDEBYZERO = unchecked((int)0x80020012); // DISP_E_DIVBYZERO
    public const int COR_E_DLLNOTFOUND = unchecked((int)0x80131524);
    public const int COR_E_DUPLICATEWAITOBJECT = unchecked((int)0x80131529);
    public const int COR_E_ENDOFSTREAM = unchecked((int)0x80070026);
    public const int COR_E_ENTRYPOINTNOTFOUND = unchecked((int)0x80131523);
    public const int COR_E_EXCEPTION = unchecked((int)0x80131500);
    public const int COR_E_EXECUTIONENGINE = unchecked((int)0x80131506);
    public const int COR_E_FAILFAST = unchecked((int)0x80131623);
    public const int COR_E_FIELDACCESS = unchecked((int)0x80131507);
    public const int COR_E_FILELOAD = unchecked((int)0x80131621);
    public const int COR_E_FILENOTFOUND = unchecked((int)0x80070002);
    public const int COR_E_FORMAT = unchecked((int)0x80131537);
    public const int COR_E_INDEXOUTOFRANGE = unchecked((int)0x80131508);
    public const int COR_E_INSUFFICIENTEXECUTIONSTACK = unchecked((int)0x80131578);
    public const int COR_E_INSUFFICIENTMEMORY = unchecked((int)0x8013153D);
    public const int COR_E_INVALIDCAST = unchecked((int)0x80004002);
    public const int COR_E_INVALIDCOMOBJECT = unchecked((int)0x80131527);
    public const int COR_E_INVALIDFILTERCRITERIA = unchecked((int)0x80131601);
    public const int COR_E_INVALIDOLEVARIANTTYPE = unchecked((int)0x80131531);
    public const int COR_E_INVALIDOPERATION = unchecked((int)0x80131509);
    public const int COR_E_INVALIDPROGRAM = unchecked((int)0x8013153A);
    public const int COR_E_IO = unchecked((int)0x80131620);
    public const int COR_E_KEYNOTFOUND = unchecked((int)0x80131577);
    public const int COR_E_MARSHALDIRECTIVE = unchecked((int)0x80131535);
    public const int COR_E_MEMBERACCESS = unchecked((int)0x8013151A);
    public const int COR_E_METHODACCESS = unchecked((int)0x80131510);
    public const int COR_E_MISSINGFIELD = unchecked((int)0x80131511);
    public const int COR_E_MISSINGMANIFESTRESOURCE = unchecked((int)0x80131532);
    public const int COR_E_MISSINGMEMBER = unchecked((int)0x80131512);
    public const int COR_E_MISSINGMETHOD = unchecked((int)0x80131513);
    public const int COR_E_MISSINGSATELLITEASSEMBLY = unchecked((int)0x80131536);
    public const int COR_E_MULTICASTNOTSUPPORTED = unchecked((int)0x80131514);
    public const int COR_E_NOTFINITENUMBER = unchecked((int)0x80131528);
    public const int COR_E_NOTSUPPORTED = unchecked((int)0x80131515);
    public const int COR_E_OBJECTDISPOSED = unchecked((int)0x80131622);
    public const int COR_E_OPERATIONCANCELED = unchecked((int)0x8013153B);
    public const int COR_E_OUTOFMEMORY = unchecked((int)0x8007000E);
    public const int COR_E_OVERFLOW = unchecked((int)0x80131516);
    public const int COR_E_PATHTOOLONG = unchecked((int)0x800700CE);
    public const int COR_E_PLATFORMNOTSUPPORTED = unchecked((int)0x80131539);
    public const int COR_E_RANK = unchecked((int)0x80131517);
    public const int COR_E_REFLECTIONTYPELOAD = unchecked((int)0x80131602);
    public const int COR_E_RUNTIMEWRAPPED = unchecked((int)0x8013153E);
    public const int COR_E_SAFEARRAYRANKMISMATCH = unchecked((int)0x80131538);
    public const int COR_E_SAFEARRAYTYPEMISMATCH = unchecked((int)0x80131533);
    public const int COR_E_SECURITY = unchecked((int)0x8013150A);
    public const int COR_E_SERIALIZATION = unchecked((int)0x8013150C);
    public const int COR_E_STACKOVERFLOW = unchecked((int)0x800703E9);
    public const int COR_E_SYNCHRONIZATIONLOCK = unchecked((int)0x80131518);
    public const int COR_E_SYSTEM = unchecked((int)0x80131501);
    public const int COR_E_TARGET = unchecked((int)0x80131603);
    public const int COR_E_TARGETINVOCATION = unchecked((int)0x80131604);
    public const int COR_E_TARGETPARAMCOUNT = unchecked((int)0x8002000E);
    public const int COR_E_THREADABORTED = unchecked((int)0x80131530);
    public const int COR_E_THREADINTERRUPTED = unchecked((int)0x80131519);
    public const int COR_E_THREADSTART = unchecked((int)0x80131525);
    public const int COR_E_THREADSTATE = unchecked((int)0x80131520);
    public const int COR_E_TIMEOUT = unchecked((int)0x80131505);
    public const int COR_E_TYPEACCESS = unchecked((int)0x80131543);
    public const int COR_E_TYPEINITIALIZATION = unchecked((int)0x80131534);
    public const int COR_E_TYPELOAD = unchecked((int)0x80131522);
    public const int COR_E_TYPEUNLOADED = unchecked((int)0x80131013);
    public const int COR_E_UNAUTHORIZEDACCESS = unchecked((int)0x80070005);
    public const int COR_E_VERIFICATION = unchecked((int)0x8013150D);
    public const int COR_E_WAITHANDLECANNOTBEOPENED = unchecked((int)0x8013152C);
    public const int CO_E_NOTINITIALIZED = unchecked((int)0x800401F0);
    public const int DISP_E_PARAMNOTFOUND = unchecked((int)0x80020004);
    public const int DISP_E_TYPEMISMATCH = unchecked((int)0x80020005);
    public const int DISP_E_BADVARTYPE = unchecked((int)0x80020008);
    public const int DISP_E_OVERFLOW = unchecked((int)0x8002000A);
    public const int DISP_E_DIVBYZERO = unchecked((int)0x80020012);
    public const int E_BOUNDS = unchecked((int)0x8000000B);
    public const int E_CHANGED_STATE = unchecked((int)0x8000000C);
    public const int E_FILENOTFOUND = unchecked((int)0x80070002);
    public const int E_FAIL = unchecked((int)0x80004005);
    public const int E_HANDLE = unchecked((int)0x80070006);
    public const int E_INVALIDARG = unchecked((int)0x80070057);
    public const int E_NOTIMPL = unchecked((int)0x80004001);
    public const int E_POINTER = unchecked((int)0x80004003);
    public const int ERROR_MRM_MAP_NOT_FOUND = unchecked((int)0x80073B1F);
    public const int ERROR_TIMEOUT = unchecked((int)0x800705B4);
    public const int RO_E_CLOSED = unchecked((int)0x80000013);
    public const int RPC_E_CHANGED_MODE = unchecked((int)0x80010106);
    public const int TYPE_E_TYPEMISMATCH = unchecked((int)0x80028CA0);
    public const int STG_E_PATHNOTFOUND = unchecked((int)0x80030003);
    public const int CTL_E_PATHNOTFOUND = unchecked((int)0x800A004C);
    public const int CTL_E_FILENOTFOUND = unchecked((int)0x800A0035);
    public const int FUSION_E_INVALID_NAME = unchecked((int)0x80131047);
    public const int FUSION_E_REF_DEF_MISMATCH = unchecked((int)0x80131040);
    public const int ERROR_TOO_MANY_OPEN_FILES = unchecked((int)0x80070004);
    public const int ERROR_SHARING_VIOLATION = unchecked((int)0x80070020);
    public const int ERROR_LOCK_VIOLATION = unchecked((int)0x80070021);
    public const int ERROR_OPEN_FAILED = unchecked((int)0x8007006E);
    public const int ERROR_DISK_CORRUPT = unchecked((int)0x80070571);
    public const int ERROR_UNRECOGNIZED_VOLUME = unchecked((int)0x800703ED);
    public const int ERROR_DLL_INIT_FAILED = unchecked((int)0x8007045A);
    public const int MSEE_E_ASSEMBLYLOADINPROGRESS = unchecked((int)0x80131016);
    public const int ERROR_FILE_INVALID = unchecked((int)0x800703EE);
}
