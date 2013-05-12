click_num = 42365;
user_num = 38024;
sku_num = 413;
click_matrix = zeros(sku_num, user_num);
basetick = datenum(['2011-08-11']);
users = zeros(click_num, 1);
skus = zeros(click_num, 1);
ticks = zeros(click_num, 1);

fid = fopen('clickinfo.txt', 'r');
for i = 1 : click_num
    users(i) = fscanf(fid, '%d', 1);
    skus(i) = fscanf(fid, '%d', 1);
    date = fscanf(fid, '%s', 1);
    time = fscanf(fid, '%s', 1);
    click_matrix(skus(i), users(i)) = 1;
    ticks(i) = datenum([date ' ' time]) - basetick;
end

fclose(fid);

fid = fopen('userid.txt', 'r');
user_names = cell(user_num, 1);
for i = 1 : user_num
    x = fscanf(fid, '%d', 1);
    user_names{i} = fscanf(fid, '%s', 1);
end
fclose(fid);

fid = fopen('skuid.txt', 'r');
sku_names = cell(sku_num, 1);
for i = 1 : sku_num
    x = fscanf(fid, '%d', 1);
    sku_names{i} = fscanf(fid, '%s', 1);
end
fclose(fid);

save('training_data', 'user_num', 'sku_num', 'click_matrix', 'users', 'skus', 'ticks', 'user_names', 'sku_names');
clear;