load training_data;

sku_count_by_day = zeros(sku_num, 28);

for i = 0 : 3 : 81
    idx = find(ticks >= i & ticks < i + 3);
    for j = 1 : sku_num
        sku_count_by_day(j, i / 3 + 1) = length(find(skus(idx) == j)) / length(idx);
    end
end

sku_count_by_hour = zeros(sku_num, 24);

tmp = mod(floor(ticks * 24), 24);

for i = 0 : 23
    idx = find(tmp == i);
    for j = 1 : sku_num
        sku_count_by_hour(j, i + 1) = length(find(skus(idx) == j)) / length(idx);
    end
end

fid = fopen('sku_day', 'w');

for i = 1 : sku_num
    for j = 1 : 28
        fprintf(fid, '%.8f ', sku_count_by_day(i, j));
    end
    fprintf(fid, '\n');
end

fclose(fid);

fid = fopen('sku_hour', 'w');

for i = 1 : sku_num
    for j = 1 : 24
        fprintf(fid, '%.8f ', sku_count_by_hour(i, j));
    end
    fprintf(fid, '\n');
end

fclose(fid);