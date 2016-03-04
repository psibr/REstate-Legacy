var gulp = require('gulp');
var runSequence = require('run-sequence');
var del = require('del');
var vinylPaths = require('vinyl-paths');
var paths = require('../paths');
var bundles = require('../bundles.json');
var resources = require('../export.json');

// deletes all files in the output path
gulp.task('clean-export', function() {
  return gulp.src([paths.exportSrv])
    .pipe(vinylPaths(del));
});

function getBundles() {
  var bl = [];
  for (b in bundles.bundles) {
    bl.push(b + '.js');
  }
  return bl;
}

function getExportList() {
  return resources.list.concat(getBundles());
}

gulp.task('export-copy', function() {
  return gulp.src(getExportList(), {base: '.'})
    .pipe(gulp.dest(paths.exportSrv));
});

// use after prepare-release
gulp.task('export', function(callback) {
  return runSequence(
    'bundle',
    'clean-export',
    'export-copy',
    callback
  );
});

gulp.task('copy-to-host', function(){
    return gulp.src([paths.exportSrv]+"**/*")
      .pipe(gulp.dest("../SelfHost/Bin/Release/wwwroot/"));
});

gulp.task('copy-to-host-debug', function(){
    return gulp.src([paths.exportSrv]+"**/*")
      .pipe(gulp.dest("../SelfHost/Bin/Debug/wwwroot/"));
});

gulp.task('host', function(callback) {
  return runSequence(
    'export',
    'copy-to-host',
    callback
  );
});

gulp.task('host-debug', function(callback) {
  return runSequence(
    'export',
    'copy-to-host-debug',
    callback
  );
});
