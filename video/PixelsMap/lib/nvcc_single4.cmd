rem ECHO OFF
rem SET TF="c:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\TF.exe"
rem %1
SET EXIT=1
SET PATH_CU=c:\projects\replica\!submodules\helpers\video\PixelsMap
FOR %%i IN ("%PATH_CU%\CUDAFunctions.cu") DO For /F "tokens=1,2,3,4,5 delims=.:/ " %%A in ("%%~ti") do SET DATE_SOURCE=%%A%%B%%C%%D%%E
FOR %%i IN ("%PATH_CU%\Properties\Resources\CUDAFunctions.20.x64.cubin") DO For /F "tokens=1,2,3,4,5 delims=.:/ " %%A in ("%%~ti") do SET DATE_TARGET=%%A%%B%%C%%D%%E
echo DATE_TARGET=%DATE_TARGET% 
echo DATE_SOURCE=%DATE_SOURCE% 
IF "%DATE_SOURCE%" GTR "%DATE_TARGET%" SET EXIT=0
echo exit = %EXIT%
rem timeout 60
IF 1==%EXIT% exit

SET NVCC="c:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v7.5\bin\\nvcc.exe"  --cl-version 2010 --cubin "%PATH_CU%\CUDAFunctions.cu" -ccbin "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\bin" -I "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\include"

rem echo checkout cubins
rem %TF% checkout "%1Properties\Resources\*.cubin"

echo compiling cubins
rem %NVCC% -m64 -arch=sm_20 -o "%PATH_CU%\Properties\Resources\CUDAFunctions.20.x64.cubin"
rem %NVCC% -m32 -arch=sm_20 -o "%PATH_CU%\Properties\Resources\CUDAFunctions.20.x32.cubin"
rem %NVCC% -m64 -arch=sm_21 -o "%PATH_CU%\Properties\Resources\CUDAFunctions.21.x64.cubin"
rem %NVCC% -m32 -arch=sm_21 -o "%PATH_CU%\Properties\Resources\CUDAFunctions.21.x32.cubin"
%NVCC% -m64 -arch=sm_30 -o "%PATH_CU%\Properties\Resources\CUDAFunctions.30.x64.cubin"
rem %NVCC% -m32 -arch=sm_30 -o "%PATH_CU%\Properties\Resources\CUDAFunctions.30.x32.cubin"
rem %NVCC% -m64 -arch=sm_35 -o "%PATH_CU%\Properties\Resources\CUDAFunctions.35.x64.cubin"
rem %NVCC% -m32 -arch=sm_35 -o "%PATH_CU%\Properties\Resources\CUDAFunctions.35.x32.cubin"
rem %NVCC% -m64 -arch=sm_50 -o "%PATH_CU%\Properties\Resources\CUDAFunctions.50.x64.cubin"
rem %NVCC% -m32 -arch=sm_50 -o "%PATH_CU%\Properties\Resources\CUDAFunctions.50.x32.cubin"
rem %NVCC% -m64 -arch=sm_52 -o "%PATH_CU%\Properties\Resources\CUDAFunctions.52.x64.cubin"