REM ���͵�ǰĿ¼�µ�����nupkg����rpa-nexus.openserver.cn��������
for %%f in (*.nupkg) do (
    nuget push %%f 3488dc1d-cb31-36fc-a9ba-5d1463c43a53 -src http://rpa-nexus.openserver.cn/repository/rpa-community-activity/
)

pause