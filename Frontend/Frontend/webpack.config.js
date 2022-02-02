const path = require('path');

module.exports = {
    entry: {
        tasks: './Scripts/tasks.ts',
        site: './Scripts/site.ts',
        edit: './Scripts/edit.ts',
        project: './Scripts/project.ts',
        index: './Scripts/index.ts',
        patch: './Scripts/patch.ts',
    },
    mode: 'production',
    optimization: {
        minimize: false
    },
    externals: {
        'three': 'THREE',
        'handlebars': 'Handlebars',
    },
    module: {
        rules: [
            {
                test: /\.tsx?$/,
                use: 'ts-loader',
                exclude: /node_modules/
            }
        ]
    },
    resolve: {
        extensions: ['.ts', '.tsx', '.js'],
        fallback: {
            "fs": false
        },
    },
    output: {
        filename: '[name].js',
        path: path.resolve(__dirname, 'wwwroot/js'),
    }
};