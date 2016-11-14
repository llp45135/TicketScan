@set NAME=TicketCode.ft.exp0
@set LANG=TicketCode

@if not exist %NAME%.box (goto LABLE_MAKEBOX) else (goto LABLE_TRIAN)


:LABLE_MAKEBOX
@echo -------begin,make box  -----------
@pause
tesseract.exe %NAME%.tif %NAME% -l TicketCode -psm 6 batch.nochop makebox
@if not exist %NAME%.box goto END_FLAG
@echo -------end,make box ,ok-----------
@pause


:LABLE_TRIAN
@echo -------begin,train   -----------
@pause
tesseract.exe %NAME%.tif %NAME% -psm 6 box.train
@pause
unicharset_extractor.exe %NAME%.box
@pause
echo ft 0 0 1 0 0 > %LANG%.font_properties
mftraining.exe -F %LANG%.font_properties -U unicharset %NAME%.tr
@pause
cntraining.exe %NAME%.tr
@pause
@rename normproto   %LANG%.normproto
@rename unicharset  %LANG%.unicharset
@rename inttemp     %LANG%.inttemp
@rename pffmtable   %LANG%.pffmtable
@rename shapetable  %LANG%.shapetable
@pause
combine_tessdata.exe %LANG%.
@echo %NAME%.traineddata
@echo ------end,succ-------------
@pause