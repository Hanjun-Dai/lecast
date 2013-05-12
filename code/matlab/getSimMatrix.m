load training_data;
sku_num = length(unique(skus));
simMatrix = zeros(sku_num);
for i = 1 : sku_num
    v = repmat(click_matrix(i, :), sku_num, 1);
    simMatrix(i, :) = (sum(v & click_matrix, 2))' ./ sum(v | click_matrix, 2)';
end

fid = fopen('simmatrix.txt', 'w');

for i = 1 : sku_num
    for j = 1 : sku_num
        fprintf(fid, '%.8f ', simMatrix(i, j));
    end
    fprintf(fid, '\n');
end

fclose(fid);