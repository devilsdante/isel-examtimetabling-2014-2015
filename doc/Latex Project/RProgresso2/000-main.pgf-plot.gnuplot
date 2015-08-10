set table "000-main.pgf-plot.table"; set format "%.5f"
set format "%.7e";; plot "saplotdata.dat" using ($0*1000):1 every 1000; 
